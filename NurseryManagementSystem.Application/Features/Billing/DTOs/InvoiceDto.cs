namespace NurseryManagementSystem.Application.Features.Billing.DTOs
{
    public record InvoiceDto(
        Guid Id,
        Guid ChildId,
        int BillingMonth,
        int BillingYear,
        decimal PlanFee,
        decimal TotalOvertimeFee,
        decimal GrandTotal,
        string Status,
        DateTime? PaidAt,
        Guid? MarkedPaidById);
}
