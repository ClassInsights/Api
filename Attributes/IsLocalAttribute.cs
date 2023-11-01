using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Attributes;

/// <inheritdoc cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class IsLocalAttribute : Attribute, IAuthorizationFilter
{
    /// <inheritdoc />
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!IsLocalRequest(context.HttpContext))
            context.Result = new UnauthorizedResult();
    }

    // https://stackoverflow.com/a/44775206/16871250
    /// <summary>
    ///     Check if request is from localhost
    /// </summary>
    /// <param name="context">Context from Request</param>
    /// <returns>True if Request is from localhost</returns>
    public static bool IsLocalRequest(HttpContext context)
    {
        return context.Connection.RemoteIpAddress != null &&
               (Equals(context.Connection.RemoteIpAddress, context.Connection.LocalIpAddress) ||
                IPAddress.IsLoopback(context.Connection.RemoteIpAddress));
    }
}