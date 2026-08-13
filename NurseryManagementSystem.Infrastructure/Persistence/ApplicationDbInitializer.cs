using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Domain.Enums;

namespace NurseryManagementSystem.Infrastructure.Persistence
{
    public class ApplicationDbInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public ApplicationDbInitializer(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_context.Database.IsRelational())
            {
                await _context.Database.MigrateAsync(cancellationToken);
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

            const string adminUserName = "superadmin";

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

                var result = await _userManager.CreateAsync(admin, "Admin@12345");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, UserRole.SuperAdmin.ToString());
                }
            }
        }
    }
}
