using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Infrastructure.Persistence
{
    public class ApplicationDbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IConfiguration _configuration;

        public ApplicationDbInitializer(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.IsRelational())
            {
                // MonsterASP databases are provisioned empty. EnsureCreated builds the
                // SQL Server schema from the current model without requiring provider-
                // specific migration files from the previous PostgreSQL deployment.
                await _context.Database.EnsureCreatedAsync(cancellationToken);
                await ApplyRegistrationSchemaUpgradeAsync(cancellationToken);
            }
        }

        private async Task ApplyRegistrationSchemaUpgradeAsync(CancellationToken cancellationToken)
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.ExecuteSqlRawAsync(SqlServerRegistrationUpgrade, cancellationToken);
                return;
            }

            if (_context.Database.IsNpgsql())
            {
                await _context.Database.ExecuteSqlRawAsync(PostgreSqlRegistrationUpgrade, cancellationToken);
            }
        }

        private const string SqlServerRegistrationUpgrade = """
            IF COL_LENGTH('AspNetUsers', 'ApprovalStatus') IS NULL
                ALTER TABLE [AspNetUsers] ADD [ApprovalStatus] int NOT NULL CONSTRAINT [DF_AspNetUsers_ApprovalStatus] DEFAULT 2;
            IF COL_LENGTH('AspNetUsers', 'ParentRelationship') IS NULL
                ALTER TABLE [AspNetUsers] ADD [ParentRelationship] int NULL;

            IF COL_LENGTH('Mothers', 'FullName') IS NULL
                ALTER TABLE [Mothers] ADD [FullName] nvarchar(200) NOT NULL CONSTRAINT [DF_Mothers_FullName] DEFAULT N'';
            IF COL_LENGTH('Fathers', 'FullName') IS NULL
                ALTER TABLE [Fathers] ADD [FullName] nvarchar(200) NOT NULL CONSTRAINT [DF_Fathers_FullName] DEFAULT N'';

            IF COL_LENGTH('Children', 'ParentUserId') IS NULL
                ALTER TABLE [Children] ADD [ParentUserId] uniqueidentifier NULL;
            IF COL_LENGTH('Children', 'ApprovalStatus') IS NULL
                ALTER TABLE [Children] ADD [ApprovalStatus] int NOT NULL CONSTRAINT [DF_Children_ApprovalStatus] DEFAULT 2;
            IF COL_LENGTH('Children', 'RequestedPlanId') IS NULL
                ALTER TABLE [Children] ADD [RequestedPlanId] uniqueidentifier NULL;
            IF COL_LENGTH('Children', 'RejectionReason') IS NULL
                ALTER TABLE [Children] ADD [RejectionReason] nvarchar(500) NULL;
            IF COL_LENGTH('Children', 'ReviewedAt') IS NULL
                ALTER TABLE [Children] ADD [ReviewedAt] datetime2 NULL;
            IF COL_LENGTH('Children', 'ReviewedById') IS NULL
                ALTER TABLE [Children] ADD [ReviewedById] uniqueidentifier NULL;
            IF COL_LENGTH('Children', 'PhotoUrl') IS NULL
                ALTER TABLE [Children] ADD [PhotoUrl] nvarchar(1000) NULL;

            IF COL_LENGTH('SubscriptionPlans', 'Category') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [Category] nvarchar(150) NOT NULL CONSTRAINT [DF_Plans_Category] DEFAULT N'Monthly Packages';
            IF COL_LENGTH('SubscriptionPlans', 'BillingCycle') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [BillingCycle] nvarchar(30) NOT NULL CONSTRAINT [DF_Plans_BillingCycle] DEFAULT N'Monthly';
            IF COL_LENGTH('SubscriptionPlans', 'DaysPerCycle') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [DaysPerCycle] int NOT NULL CONSTRAINT [DF_Plans_DaysPerCycle] DEFAULT 5;
            IF COL_LENGTH('SubscriptionPlans', 'IsFullDay') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [IsFullDay] bit NOT NULL CONSTRAINT [DF_Plans_IsFullDay] DEFAULT 0;
            IF COL_LENGTH('SubscriptionPlans', 'BadgeText') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [BadgeText] nvarchar(80) NULL;
            IF COL_LENGTH('SubscriptionPlans', 'IsFeatured') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [IsFeatured] bit NOT NULL CONSTRAINT [DF_Plans_IsFeatured] DEFAULT 0;
            IF COL_LENGTH('SubscriptionPlans', 'IsActive') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_Plans_IsActive] DEFAULT 1;
            IF COL_LENGTH('SubscriptionPlans', 'Currency') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [Currency] nvarchar(3) NOT NULL CONSTRAINT [DF_Plans_Currency] DEFAULT N'AED';
            IF COL_LENGTH('SubscriptionPlans', 'DisplayOrder') IS NULL
                ALTER TABLE [SubscriptionPlans] ADD [DisplayOrder] int NOT NULL CONSTRAINT [DF_Plans_DisplayOrder] DEFAULT 0;

            IF COL_LENGTH('ChildPlanAssignments', 'AssignedAt') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [AssignedAt] datetime2 NOT NULL CONSTRAINT [DF_Assignments_AssignedAt] DEFAULT SYSUTCDATETIME();
            IF COL_LENGTH('ChildPlanAssignments', 'PlanNameSnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [PlanNameSnapshot] nvarchar(150) NOT NULL CONSTRAINT [DF_Assignments_PlanName] DEFAULT N'';
            IF COL_LENGTH('ChildPlanAssignments', 'PlanCategorySnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [PlanCategorySnapshot] nvarchar(150) NOT NULL CONSTRAINT [DF_Assignments_Category] DEFAULT N'';
            IF COL_LENGTH('ChildPlanAssignments', 'PriceSnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [PriceSnapshot] decimal(18,2) NOT NULL CONSTRAINT [DF_Assignments_Price] DEFAULT 0;
            IF COL_LENGTH('ChildPlanAssignments', 'DurationHoursSnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [DurationHoursSnapshot] int NOT NULL CONSTRAINT [DF_Assignments_Hours] DEFAULT 0;
            IF COL_LENGTH('ChildPlanAssignments', 'DaysPerCycleSnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [DaysPerCycleSnapshot] int NOT NULL CONSTRAINT [DF_Assignments_Days] DEFAULT 0;
            IF COL_LENGTH('ChildPlanAssignments', 'CurrencySnapshot') IS NULL
                ALTER TABLE [ChildPlanAssignments] ADD [CurrencySnapshot] nvarchar(3) NOT NULL CONSTRAINT [DF_Assignments_Currency] DEFAULT N'AED';

            IF COL_LENGTH('MonthlyInvoices', 'AdjustmentAmount') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [AdjustmentAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Invoices_Adjustment] DEFAULT 0;
            IF COL_LENGTH('MonthlyInvoices', 'AdjustmentReason') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [AdjustmentReason] nvarchar(1000) NULL;
            IF COL_LENGTH('MonthlyInvoices', 'PenaltyAmount') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [PenaltyAmount] decimal(18,2) NOT NULL CONSTRAINT [DF_Invoices_Penalty] DEFAULT 0;
            IF COL_LENGTH('MonthlyInvoices', 'LatePickupDays') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [LatePickupDays] int NOT NULL CONSTRAINT [DF_Invoices_LateDays] DEFAULT 0;
            IF COL_LENGTH('MonthlyInvoices', 'LatePickupFinePerDay') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [LatePickupFinePerDay] decimal(18,2) NOT NULL CONSTRAINT [DF_Invoices_LateFine] DEFAULT 0;
            IF COL_LENGTH('MonthlyInvoices', 'OvertimeRate') IS NULL
                ALTER TABLE [MonthlyInvoices] ADD [OvertimeRate] decimal(18,2) NOT NULL CONSTRAINT [DF_Invoices_OvertimeRate] DEFAULT 0;

            IF OBJECT_ID(N'NurserySettings', N'U') IS NULL
            BEGIN
                CREATE TABLE [NurserySettings] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY,
                    [NurseryName] nvarchar(200) NOT NULL,
                    [Capacity] int NOT NULL,
                    [Currency] nvarchar(3) NOT NULL,
                    [OvertimeHourlyRate] decimal(18,2) NOT NULL,
                    [LatePickupGraceMinutes] int NOT NULL,
                    [LatePickupFinePerDay] decimal(18,2) NOT NULL,
                    [OpeningTime] time NOT NULL,
                    [ClosingTime] time NOT NULL,
                    [TimeZone] nvarchar(100) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    [CreatedBy] uniqueidentifier NULL,
                    [UpdatedAt] datetime2 NULL,
                    [UpdatedBy] uniqueidentifier NULL
                );
            END;

            IF OBJECT_ID(N'AuditLogEntries', N'U') IS NULL
            BEGIN
                CREATE TABLE [AuditLogEntries] (
                    [Id] uniqueidentifier NOT NULL PRIMARY KEY, [At] datetime2 NOT NULL,
                    [ActorUserId] uniqueidentifier NULL, [ActorName] nvarchar(256) NOT NULL,
                    [Action] nvarchar(100) NOT NULL, [SubjectType] nvarchar(100) NOT NULL,
                    [SubjectId] uniqueidentifier NOT NULL, [SubjectName] nvarchar(256) NULL,
                    [Amount] decimal(18,2) NULL, [Details] nvarchar(2000) NULL
                );
                CREATE INDEX [IX_AuditLogEntries_At] ON [AuditLogEntries] ([At]);
            END;

            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Children_ParentUserId_ApprovalStatus' AND object_id = OBJECT_ID('Children'))
                CREATE INDEX [IX_Children_ParentUserId_ApprovalStatus] ON [Children] ([ParentUserId], [ApprovalStatus]);
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Children_AspNetUsers_ParentUserId')
                ALTER TABLE [Children] ADD CONSTRAINT [FK_Children_AspNetUsers_ParentUserId]
                    FOREIGN KEY ([ParentUserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL;
            """;

        private const string PostgreSqlRegistrationUpgrade = """
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "ApprovalStatus" integer NOT NULL DEFAULT 2;
            ALTER TABLE "AspNetUsers" ADD COLUMN IF NOT EXISTS "ParentRelationship" integer NULL;
            ALTER TABLE "Mothers" ADD COLUMN IF NOT EXISTS "FullName" character varying(200) NOT NULL DEFAULT '';
            ALTER TABLE "Fathers" ADD COLUMN IF NOT EXISTS "FullName" character varying(200) NOT NULL DEFAULT '';
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "ParentUserId" uuid NULL;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "ApprovalStatus" integer NOT NULL DEFAULT 2;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "RequestedPlanId" uuid NULL;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "RejectionReason" character varying(500) NULL;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "ReviewedAt" timestamp with time zone NULL;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "ReviewedById" uuid NULL;
            ALTER TABLE "Children" ADD COLUMN IF NOT EXISTS "PhotoUrl" character varying(1000) NULL;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "Category" character varying(150) NOT NULL DEFAULT 'Monthly Packages';
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "BillingCycle" character varying(30) NOT NULL DEFAULT 'Monthly';
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "DaysPerCycle" integer NOT NULL DEFAULT 5;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "IsFullDay" boolean NOT NULL DEFAULT false;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "BadgeText" character varying(80) NULL;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "IsFeatured" boolean NOT NULL DEFAULT false;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "IsActive" boolean NOT NULL DEFAULT true;
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "Currency" character varying(3) NOT NULL DEFAULT 'AED';
            ALTER TABLE "SubscriptionPlans" ADD COLUMN IF NOT EXISTS "DisplayOrder" integer NOT NULL DEFAULT 0;
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "AssignedAt" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP;
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "PlanNameSnapshot" character varying(150) NOT NULL DEFAULT '';
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "PlanCategorySnapshot" character varying(150) NOT NULL DEFAULT '';
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "PriceSnapshot" numeric(18,2) NOT NULL DEFAULT 0;
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "DurationHoursSnapshot" integer NOT NULL DEFAULT 0;
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "DaysPerCycleSnapshot" integer NOT NULL DEFAULT 0;
            ALTER TABLE "ChildPlanAssignments" ADD COLUMN IF NOT EXISTS "CurrencySnapshot" character varying(3) NOT NULL DEFAULT 'AED';
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "AdjustmentAmount" numeric(18,2) NOT NULL DEFAULT 0;
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "AdjustmentReason" character varying(1000) NULL;
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "PenaltyAmount" numeric(18,2) NOT NULL DEFAULT 0;
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "LatePickupDays" integer NOT NULL DEFAULT 0;
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "LatePickupFinePerDay" numeric(18,2) NOT NULL DEFAULT 0;
            ALTER TABLE "MonthlyInvoices" ADD COLUMN IF NOT EXISTS "OvertimeRate" numeric(18,2) NOT NULL DEFAULT 0;
            CREATE TABLE IF NOT EXISTS "NurserySettings" (
                "Id" uuid NOT NULL PRIMARY KEY, "NurseryName" character varying(200) NOT NULL,
                "Capacity" integer NOT NULL, "Currency" character varying(3) NOT NULL,
                "OvertimeHourlyRate" numeric(18,2) NOT NULL, "LatePickupGraceMinutes" integer NOT NULL,
                "LatePickupFinePerDay" numeric(18,2) NOT NULL, "OpeningTime" time NOT NULL,
                "ClosingTime" time NOT NULL, "TimeZone" character varying(100) NOT NULL,
                "CreatedAt" timestamp with time zone NOT NULL, "CreatedBy" uuid NULL,
                "UpdatedAt" timestamp with time zone NULL, "UpdatedBy" uuid NULL);
            CREATE TABLE IF NOT EXISTS "AuditLogEntries" (
                "Id" uuid NOT NULL PRIMARY KEY, "At" timestamp with time zone NOT NULL,
                "ActorUserId" uuid NULL, "ActorName" character varying(256) NOT NULL,
                "Action" character varying(100) NOT NULL, "SubjectType" character varying(100) NOT NULL,
                "SubjectId" uuid NOT NULL, "SubjectName" character varying(256) NULL,
                "Amount" numeric(18,2) NULL, "Details" character varying(2000) NULL);
            CREATE INDEX IF NOT EXISTS "IX_AuditLogEntries_At" ON "AuditLogEntries" ("At");
            CREATE INDEX IF NOT EXISTS "IX_Children_ParentUserId_ApprovalStatus"
                ON "Children" ("ParentUserId", "ApprovalStatus");
            """;

        public async Task SeedAsync()
        {
            foreach (var role in Enum.GetNames<UserRole>())
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            var adminUserName = _configuration["AdminSeed:UserName"];
            var adminPassword = _configuration["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(adminUserName) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            if (await _userManager.FindByNameAsync(adminUserName) is null)
            {
                var admin = new AppUser
                {
                    UserName = adminUserName,
                    FullName = "System Administrator",
                    Role = UserRole.SuperAdmin,
                    IsActive = true,
                    QrCode = $"STF-{Guid.NewGuid():N}",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Unable to create the initial admin user: {errors}");
                }

                await _userManager.AddToRoleAsync(admin, UserRole.SuperAdmin.ToString());
            }
        }
    }
}
