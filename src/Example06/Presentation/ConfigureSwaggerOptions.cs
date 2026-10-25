using Example06.Presentation.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Example06.Presentation;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(JwtConstants.BearerScheme, new OpenApiSecurityScheme
        {
            Description = "A JWT access token, sent as 'Authorization: Bearer {token}'",
            Scheme = JwtConstants.BearerScheme,
            BearerFormat = "JWT",
            Type = SecuritySchemeType.Http
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(JwtConstants.BearerScheme, document)] = []
        });
    }
}
