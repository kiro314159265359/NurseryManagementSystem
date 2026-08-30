using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Attendance;
using NurseryManagementSystem.Domain.Entities.Billing;
using NurseryManagementSystem.Domain.Entities.Children;
using NurseryManagementSystem.Domain.Entities.Plans;
using NurseryManagementSystem.Domain.Enums;
using NurseryManagementSystem.Domain.Entities.Nursery;

namespace NurseryManagementSystem.Application.Features.Billing.Commands
{
    public record GenerateMonthlyInvoicesCommand(int Month, int Year) : IRequest<int>;

    public class GenerateMonthlyInvoicesCommandValidator : AbstractValidator<GenerateMonthlyInvoicesCommand>
    {
        public GenerateMonthlyInvoicesCommandValidator()
        {
            RuleFor(x => x.Month).InclusiveBetween(1, 12);
            RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        }
    }

    public class GenerateMonthlyInvoicesCommandHandler : IRequestHandler<GenerateMonthlyInvoicesCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GenerateMonthlyInvoicesCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(GenerateMonthlyInvoicesCommand request, CancellationToken cancellationToken)
        {
            var startDate = new DateOnly(request.Year, request.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var activeChildIds = await _unitOfWork.Repository<Child>().Query()
                .AsNoTracking()
                .Where(c => c.IsActive && c.ApprovalStatus == ApprovalStatus.Approved)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);

            var invoiceRepo = _unitOfWork.Repository<MonthlyInvoice>();

            var alreadyInvoiced = await invoiceRepo.Query()
                .AsNoTracking()
                .Where(i => i.BillingYear == request.Year && i.BillingMonth == request.Month)
                .Select(i => i.ChildId)
                .ToListAsync(cancellationToken);

            var alreadyInvoicedSet = alreadyInvoiced.ToHashSet();

            var assignmentRepo = _unitOfWork.Repository<ChildPlanAssignment>();
            var attendanceRepo = _unitOfWork.Repository<ChildAttendance>();

            var generated = 0;
            var settings = await _unitOfWork.Repository<NurserySettings>().Query()
                .AsNoTracking().FirstOrDefaultAsync(cancellationToken);

            foreach (var childId in activeChildIds)
            {
                if (alreadyInvoicedSet.Contains(childId))
                {
                    continue;
                }

                var assignment = await assignmentRepo.Query()
                    .AsNoTracking()
                    .Include(a => a.Plan)
                    .Where(a => a.ChildId == childId
                                && a.StartDate <= endDate
                                && (a.EndDate == null || a.EndDate >= startDate))
                    .OrderByDescending(a => a.StartDate)
                    .FirstOrDefaultAsync(cancellationToken);

                var planFee = assignment?.Plan?.MonthlyFee ?? 0m;

                var attendanceRows = await attendanceRepo.Query()
                    .AsNoTracking()
                    .Where(a => a.ChildId == childId
                                && a.AttendanceDate >= startDate
                                && a.AttendanceDate <= endDate)
                    .ToListAsync(cancellationToken);
                var totalOvertime = attendanceRows.Sum(a => a.OvertimeFee);
                var graceHours = (settings?.LatePickupGraceMinutes ?? 0) / 60m;
                var latePickupDays = attendanceRows.Count(a => a.OvertimeHours > graceHours);
                var finePerDay = settings?.LatePickupFinePerDay ?? 0m;
                var penaltyAmount = latePickupDays * finePerDay;
                var overtimeRate = assignment?.Plan?.DailyOvertimeFee > 0
                    ? assignment.Plan.DailyOvertimeFee
                    : settings?.OvertimeHourlyRate ?? 0m;

                var invoice = new MonthlyInvoice
                {
                    ChildId = childId,
                    BillingMonth = request.Month,
                    BillingYear = request.Year,
                    PlanFee = planFee,
                    TotalOvertimeFee = totalOvertime,
                    PenaltyAmount = penaltyAmount,
                    LatePickupDays = latePickupDays,
                    LatePickupFinePerDay = finePerDay,
                    OvertimeRate = overtimeRate,
                    GrandTotal = planFee + totalOvertime + penaltyAmount,
                    Status = InvoiceStatus.Pending
                };

                await invoiceRepo.AddAsync(invoice, cancellationToken);
                generated++;
            }

            if (generated > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return generated;
        }
    }
}
