using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseryManagementSystem.Application.Common.Models;
using NurseryManagementSystem.Application.Features.Billing.Commands;
using NurseryManagementSystem.Application.Features.Billing.DTOs;
using NurseryManagementSystem.Application.Features.Billing.Queries;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.API.Controllers
{
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public class BillingController : ApiControllerBase
    {
        [HttpPost("generate")]
        public async Task<IActionResult> Generate(GenerateMonthlyInvoicesCommand command)
        {
            var count = await Mediator.Send(command);
            return Ok(new { generated = count });
        }

        [HttpGet("invoices")]
        public async Task<ActionResult<PaginatedList<InvoiceDto>>> GetInvoices(
            Guid? childId = null,
            InvoiceStatus? status = null,
            int? year = null,
            int? month = null,
            int pageNumber = 1,
            int pageSize = 20)
            => Ok(await Mediator.Send(new GetInvoicesQuery(childId, status, year, month, pageNumber, pageSize)));

        [HttpGet("invoices/{id:guid}")]
        public async Task<ActionResult<InvoiceDto>> GetInvoice(Guid id)
            => Ok(await Mediator.Send(new GetInvoiceByIdQuery(id)));

        [HttpPut("invoices/{id:guid}/pay")]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            await Mediator.Send(new MarkInvoicePaidCommand(id));
            return NoContent();
        }

        [HttpPut("invoices/{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await Mediator.Send(new CancelInvoiceCommand(id));
            return NoContent();
        }
    }
}
