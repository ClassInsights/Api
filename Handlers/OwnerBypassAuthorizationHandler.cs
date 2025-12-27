using Microsoft.AspNetCore.Authorization;

namespace Api.Handlers;

public class OwnerBypassAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (!context.User.IsInRole("Owner"))
            return Task.CompletedTask;

        foreach (var requirement in context.PendingRequirements.ToList())
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}