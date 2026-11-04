using DonationHub.Application.Common.Abstractions;
using DonationHub.Application.Features.Admin;
using DonationHub.Application.Features.Auth;
using DonationHub.Application.Features.Categories;
using DonationHub.Domain.Donations;
using DonationHub.Infrastructure.Identity;
using DonationHub.Infrastructure.Interceptors;
using DonationHub.Infrastructure.Options;
using DonationHub.Infrastructure.Persistence;
using DonationHub.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
        {
            // Options (Validation + Fail fast)
            services.AddValidatedOptions<DatabaseOptions>(configuration, DatabaseOptions.SectionName);
            services.AddValidatedOptions<JwtOptions>(configuration, JwtOptions.SectionName);
            services.AddValidatedOptions<SeedAdminOptions>(configuration, SeedAdminOptions.SectionName);
            services.AddValidatedOptions<DonationOptions>(configuration, DonationOptions.SectionName);

            // Common
            services.AddSingleton(TimeProvider.System);

            // Persistence
            services.AddSingleton<SoftDeleteInterceptor>();

            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                options.UseSqlServer(db.ConnectionString)
                       .AddInterceptors(sp.GetRequiredService<SoftDeleteInterceptor>());
            });
            // Identity
            services.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireDigit = true;
                o.Password.RequireNonAlphanumeric = false;
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>();

            // Category services
            services.AddScoped<IAsyncQueryExecutor, AsyncQueryExecutor>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            // Auth services
            services.AddSingleton<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IStaffService, StaffService>();

            // Seeding
            services.AddOptions<SeedDevUsersOptions>()
                .Bind(configuration.GetSection(SeedDevUsersOptions.SectionName));
            services.AddHostedService<DatabaseSeederHostedService>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReadDbContext, ReadDbContext>();

            return services;
        }
    }
}
