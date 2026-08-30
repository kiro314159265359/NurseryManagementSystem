using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Nursery;

namespace NurseryManagementSystem.API.Controllers;

[Route("api/nursery/settings")]
[ApiController]
[Authorize(Roles = "SuperAdmin,SubAdmin")]
public class NurseryController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public NurseryController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<ActionResult<NurserySettingsDto>> Get(CancellationToken cancellationToken)
        => Ok(ToDto(await GetOrCreate(cancellationToken)));

    [HttpPut]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<NurserySettingsDto>> Update(
        UpdateNurserySettingsRequest request,
        CancellationToken cancellationToken)
    {
        var settings = await GetOrCreate(cancellationToken);
        settings.NurseryName = request.NurseryName.Trim();
        settings.Capacity = request.Capacity;
        settings.Currency = request.Currency.Trim().ToUpperInvariant();
        settings.OvertimeHourlyRate = request.OvertimeHourlyRate;
        settings.LatePickupGraceMinutes = request.LatePickupGraceMinutes;
        settings.LatePickupFinePerDay = request.LatePickupFinePerDay;
        settings.OpeningTime = request.OpeningTime;
        settings.ClosingTime = request.ClosingTime;
        settings.TimeZone = request.TimeZone.Trim();
        _unitOfWork.Repository<NurserySettings>().Update(settings);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(settings));
    }

    private async Task<NurserySettings> GetOrCreate(CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.Repository<NurserySettings>().Query()
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (settings is not null) return settings;
        settings = new NurserySettings();
        await _unitOfWork.Repository<NurserySettings>().AddAsync(settings, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return settings;
    }

    private static NurserySettingsDto ToDto(NurserySettings value) => new(
        value.NurseryName, value.Capacity, value.Currency, value.OvertimeHourlyRate,
        value.LatePickupGraceMinutes, value.LatePickupFinePerDay,
        value.OpeningTime, value.ClosingTime, value.TimeZone);
}

public record NurserySettingsDto(
    string NurseryName, int Capacity, string Currency, decimal OvertimeHourlyRate,
    int LatePickupGraceMinutes, decimal LatePickupFinePerDay,
    TimeOnly OpeningTime, TimeOnly ClosingTime, string TimeZone);

public record UpdateNurserySettingsRequest(
    string NurseryName, int Capacity, string Currency, decimal OvertimeHourlyRate,
    int LatePickupGraceMinutes, decimal LatePickupFinePerDay,
    TimeOnly OpeningTime, TimeOnly ClosingTime, string TimeZone);
