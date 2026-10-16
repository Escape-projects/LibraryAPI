using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace LibraryAPI.OpenApi
{
    public class SecuritySchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var securityScheme = new OpenApiSecurityScheme{
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Введите JWT-токен (получить через POST /api/auth/login)"
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = securityScheme;

            // Применяем схему ко ВСЕМ endpoint'ам по умолчанию
            var securityRequirement = new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
            };

            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(securityRequirement);

            return Task.CompletedTask;
        }
    }
}
