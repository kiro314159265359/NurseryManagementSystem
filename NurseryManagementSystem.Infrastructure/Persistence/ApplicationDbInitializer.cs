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
            }
        }

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
