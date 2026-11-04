using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace DonationHub.Api.OpenApi
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider schemeProvider)
    : IOpenApiDocumentTransformer
    {
        private const string SchemeName = "Bearer";

        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            // لو مفيش JWT Bearer متسجل، منعملش حاجة
            var schemes = await schemeProvider.GetAllSchemesAsync();
            if (!schemes.Any(s => s.Name == SchemeName))
                return;

            // 1) تعريف الـ Security Scheme (زر Authorize)
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",          // لازم lowercase
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            };

            foreach (var operation in document.Paths.Values.SelectMany(p => p.Operations ?? []))
            {
                operation.Value.Security ??= [];
                operation.Value.Security.Add(new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(SchemeName, document)] = []
                });
            }
        }
    }
}
