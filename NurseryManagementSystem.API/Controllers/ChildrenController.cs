using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Children.Commands;
using NurseryManagementSystem.Application.Features.Children.DTOs;
using NurseryManagementSystem.Application.Features.Children.Queries;
using NurseryManagementSystem.Application.Common.Exceptions;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class ChildrenController : ApiControllerBase
    {
        private static readonly HashSet<string> AllowedPhotoTypes =
            new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public ChildrenController(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _environment = environment;
        }
        [HttpGet]
        public async Task<ActionResult<PaginatedList<ChildDto>>> Get(
            int pageNumber = 1,
            int pageSize = 20,
            string? search = null,
            bool activeOnly = false)
            => Ok(await Mediator.Send(new GetChildrenQuery(pageNumber, pageSize, search, activeOnly)));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ChildDetailsDto>> GetById(Guid id)
            => Ok(await Mediator.Send(new GetChildByIdQuery(id)));

        [HttpPost]
        public async Task<IActionResult> Create(CreateChildCommand command)
        {
            var id = await Mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id }, new { id });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, UpdateChildCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/active")]
        public async Task<IActionResult> SetActive(Guid id, SetChildActiveCommand command)
        {
            await Mediator.Send(command with { Id = id });
            return NoContent();
        }

        [HttpPut("{id:guid}/status")]
        public async Task<IActionResult> SetStatus(Guid id, SetChildStatusRequest request)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(id)
                ?? throw new NotFoundException("Child", id);
            switch (request.Status.Trim().ToLowerInvariant())
            {
                case "active":
                    child.ApprovalStatus = ApprovalStatus.Approved;
                    child.IsActive = true;
                    break;
                case "inactive":
                case "withdrawn":
                    child.IsActive = false;
                    break;
                case "pending":
                    child.ApprovalStatus = ApprovalStatus.Pending;
                    child.IsActive = false;
                    break;
                case "rejected":
                    child.ApprovalStatus = ApprovalStatus.Rejected;
                    child.IsActive = false;
                    break;
                default:
                    throw new ConflictException("Unknown child status. Use Active, Inactive, Pending, or Rejected.");
            }
            _unitOfWork.Repository<Child>().Update(child);
            await _unitOfWork.SaveChangesAsync();
            return Ok(await Mediator.Send(new GetChildByIdQuery(id)));
        }

        [HttpPost("{id:guid}/scan-code/regenerate")]
        public async Task<IActionResult> RegenerateScanCode(Guid id)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(id)
                ?? throw new NotFoundException("Child", id);
            child.QrCode = $"CHD-{Guid.NewGuid():N}";
            _unitOfWork.Repository<Child>().Update(child);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { scanCode = child.QrCode, issuedAt = DateTime.UtcNow });
        }

        [HttpPost("{id:guid}/photo")]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(id)
                ?? throw new NotFoundException("Child", id);
            if (file.Length == 0 || file.Length > 5 * 1024 * 1024 || !AllowedPhotoTypes.Contains(file.ContentType))
            {
                throw new ConflictException("Photo must be a JPEG, PNG, or WebP image no larger than 5 MB.");
            }

            var extension = file.ContentType.ToLowerInvariant() switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                _ => ".webp"
            };
            var root = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var directory = Path.Combine(root, "uploads", "children");
            Directory.CreateDirectory(directory);
            var fileName = $"{id:N}-{Guid.NewGuid():N}{extension}";
            await using (var stream = System.IO.File.Create(Path.Combine(directory, fileName)))
            {
                await file.CopyToAsync(stream);
            }
            child.PhotoUrl = $"/uploads/children/{fileName}";
            _unitOfWork.Repository<Child>().Update(child);
            await _unitOfWork.SaveChangesAsync();
            return Ok(new { photoUrl = $"{Request.Scheme}://{Request.Host}{child.PhotoUrl}" });
        }

        [HttpDelete("{id:guid}/photo")]
        public async Task<IActionResult> DeletePhoto(Guid id)
        {
            var child = await _unitOfWork.Repository<Child>().GetByIdAsync(id)
                ?? throw new NotFoundException("Child", id);
            child.PhotoUrl = null;
            _unitOfWork.Repository<Child>().Update(child);
            await _unitOfWork.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("{childId:guid}/emergency-contacts")]
        public async Task<IActionResult> AddEmergencyContact(Guid childId, AddEmergencyContactCommand command)
        {
            var id = await Mediator.Send(command with { ChildId = childId });
            return Ok(new { id });
        }

        [HttpDelete("{childId:guid}/emergency-contacts/{contactId:guid}")]
        public async Task<IActionResult> RemoveEmergencyContact(Guid childId, Guid contactId)
        {
            await Mediator.Send(new RemoveEmergencyContactCommand(childId, contactId));
            return NoContent();
        }
    }

    public record SetChildStatusRequest(string Status);
}
