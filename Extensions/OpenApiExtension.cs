using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace Api.Extensions;

public static class OpenApiExtension
{
    public static void AddBearerTokenAuthentication(this OpenApiOptions options)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Name = IdentityConstants.BearerScheme,
            Scheme = "Bearer",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = IdentityConstants.BearerScheme
            }
        };
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "ClassInsights API",
                Version = "v1",
                Description =
                    "This is the local API for ClassInsights, which is used to manage the data of the ClassInsights Dashboard and Clients. It is not intended to be used by external users."
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes.Add(IdentityConstants.BearerScheme, scheme);
            return Task.CompletedTask;
        });
        options.AddOperationTransformer((operation, context, _) =>
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any())
                operation.Security = [new OpenApiSecurityRequirement { [scheme] = [] }];
            return Task.CompletedTask;
        });
    }
}