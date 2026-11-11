using Microsoft.EntityFrameworkCore;
using DonationHub.Domain.Categories;
using DonationHub.Domain.Users;
using DonationHub.Infrastructure.Identity;
using DonationHub.Infrastructure.Options;
using DonationHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Seeding
{
    internal sealed class DatabaseSeederHostedService(
    IServiceProvider services,
    IHostEnvironment env) : IHostedService
    {
        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var clock = sp.GetRequiredService<TimeProvider>();
            var now = clock.GetUtcNow().UtcDateTime;
            var users = sp.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
            await SeedAdminAsync(users, sp.GetRequiredService<IOptions<SeedAdminOptions>>().Value, now);

            if (env.IsDevelopment())
            {
                await SeedDevUsersAsync(users, sp.GetRequiredService<IOptions<SeedDevUsersOptions>>().Value, now);
                await SeedCategoriesAsync(sp.GetRequiredService<AppDbContext>(), now, ct);
            }
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;

        private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roles)
        {
            foreach (var role in new[] { Roles.Admin, Roles.CampaignManager, Roles.Donor })
            {
                if (!await roles.RoleExistsAsync(role))
                    await roles.CreateAsync(new IdentityRole<Guid>(role) { Id = Guid.NewGuid() });
            }
        }

        private static async Task SeedAdminAsync(
            UserManager<ApplicationUser> users, SeedAdminOptions admin, DateTime now)
        {
            if (await users.FindByEmailAsync(admin.Email) is null)
                await UserCreation.CreateOrThrowAsync(
                    users, admin.Email, admin.FullName, admin.Password, Languages.English, Roles.Admin, now);
        }

        private static async Task SeedDevUsersAsync(
            UserManager<ApplicationUser> users, SeedDevUsersOptions dev, DateTime now)
        {
            await SeedIfConfiguredAsync(users, dev.CampaignManagerEmail, dev.CampaignManagerPassword,
                "Test Campaign Manager", Roles.CampaignManager, now);

            await SeedIfConfiguredAsync(users, dev.DonorEmail, dev.DonorPassword,
                "Test Donor", Roles.Donor, now);
        }

        private static async Task SeedIfConfiguredAsync(
            UserManager<ApplicationUser> users, string email, string password,
            string fullName, string role, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;
            if (await users.FindByEmailAsync(email) is not null) return;

            await UserCreation.CreateOrThrowAsync(users, email, fullName, password, Languages.English, role, now);
        }

        private static async Task SeedCategoriesAsync(AppDbContext db, DateTime now, CancellationToken ct)
        {
            if (await db.Categories.AnyAsync(ct)) return;

            db.Categories.AddRange(
                Category.Create("الصحة", "Health", "دعم العلاج والرعاية الطبية", "Support for treatment and medical care", now),
                Category.Create("التعليم", "Education", "تمويل التعليم والمنح الدراسية", "Funding education and scholarships", now),
                Category.Create("الإغاثة", "Relief", "المساعدة في حالات الطوارئ والكوارث", "Emergency and disaster assistance", now),
                Category.Create("الأيتام", "Orphans", "رعاية الأيتام وكفالتهم", "Care and sponsorship for orphans", now),
                Category.Create("البيئة", "Environment", "حماية البيئة والمشروعات المستدامة", "Environmental protection and sustainability", now));

            await db.SaveChangesAsync(ct);
        }
    }
}
