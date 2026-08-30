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
