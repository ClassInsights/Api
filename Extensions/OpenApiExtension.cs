using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.Extensions;

public static class OpenApiExtension
{
    public static void AddBearerTokenAuthentication(this OpenApiOptions options)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Name = IdentityConstants.BearerScheme,
            Scheme = "Bearer"
        };

        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Info = new OpenApiInfo
            {
                Title = "ClassInsights API",
                Version = "v1",
                Description =
                    "This is the local API for ClassInsights, which is used to manage the data of the ClassInsights Dashboard and Clients. It is not intended to be used by external users."
            };

            document.Components.SecuritySchemes.Add(IdentityConstants.BearerScheme, scheme);
            return Task.CompletedTask;
        });
    }
}