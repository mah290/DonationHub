using DonationHub.Application.Common.Behaviors;
using DonationHub.Application.Features.Categories;
using DonationHub.Application.Features.Auth;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
            services.AddScoped<TokenIssuer>();
            return services;
        }
    }
}
