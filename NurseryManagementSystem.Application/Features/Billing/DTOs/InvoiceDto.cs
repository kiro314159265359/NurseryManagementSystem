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
        Guid? MarkedPaidById,
        string? InvoiceNumber = null,
        string? ChildFullName = null,
        string? ParentFullName = null,
        string? ParentPhone = null,
        string Currency = "AED",
        decimal AmountPaid = 0,
        decimal Outstanding = 0,
        string? PlanName = null,
        DateTime? CreatedAt = null,
        decimal AdjustmentAmount = 0,
        string? AdjustmentReason = null,
        decimal PenaltyAmount = 0,
        int LatePickupDays = 0,
        decimal LatePickupFinePerDay = 0,
        decimal OvertimeRate = 0,
        decimal OvertimeHours = 0,
        Guid? PlanId = null,
        DateOnly? DueDate = null,
        string? PaidByName = null)
    {
        public decimal BaseFee => PlanFee;
        public decimal OvertimeAmount => TotalOvertimeFee;
        public decimal TotalDue => GrandTotal;
    }
}
