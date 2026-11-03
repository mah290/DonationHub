using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonationHub.Infrastructure.Options
{
    internal static class OptionsExtensions
    {
        public static IServiceCollection AddValidatedOptions<TOptions>(
       this IServiceCollection services,
       IConfiguration configuration,
       string sectionName)
       where TOptions : class
        {
            services.AddOptions<TOptions>()
                .Bind(configuration.GetSection(sectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }
    }
}
