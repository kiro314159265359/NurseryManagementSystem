using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NurseryManagementSystem.Application.Common.Interfaces;
using NurseryManagementSystem.Domain.Entities.Identity;
using NurseryManagementSystem.Infrastructure.Identity;
using NurseryManagementSystem.Infrastructure.Persistence;
using NurseryManagementSystem.Infrastructure.Persistence.Interceptors;
using NurseryManagementSystem.Infrastructure.Persistence.Repositories;
using NurseryManagementSystem.Infrastructure.Services;

namespace NurseryManagementSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                // Remove debug console log for production
                // Console.WriteLine($"EF CONNECTION: {connectionString}");
                options.UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
            });

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services
                .AddIdentityCore<AppUser>(options =>
                {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = false;
                })
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IIdentityService, IdentityService>();

            services.AddScoped<ApplicationDbInitializer>();

            AddJwtAuthentication(services, configuration);

            return services;
        }

        private static void AddJwtAuthentication(
            IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(JwtSettings.SectionName);
            services.Configure<JwtSettings>(section);

            var jwtSettings = section.Get<JwtSettings>() ?? new JwtSettings();

            // For Render deployment, prioritize environment variables
            var secretKey = Environment.GetEnvironmentVariable("Jwt__SecretKey") ?? jwtSettings.SecretKey;
            var issuer = Environment.GetEnvironmentVariable("Jwt__Issuer") ?? jwtSettings.Issuer;
            var audience = Environment.GetEnvironmentVariable("Jwt__Audience") ?? jwtSettings.Audience;

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();
        }
    }
}
