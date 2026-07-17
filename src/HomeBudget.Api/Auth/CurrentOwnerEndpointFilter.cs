using HomeBudget.Infrastructure.Server.Identity;
using System.Security.Claims;

namespace HomeBudget.Api.Auth;

internal sealed class CurrentOwnerEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (httpContext.User.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Challenge();
        }

        var issuer = httpContext.User.FindFirstValue("iss");
        var subject = httpContext.User.FindFirstValue("sub")
            ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject))
        {
            return TypedResults.Forbid();
        }

        var userAccountRepository = httpContext.RequestServices.GetRequiredService<IUserAccountRepository>();
        var userAccount = await userAccountRepository.GetByIssuerAndSubjectAsync(
            issuer,
            subject,
            httpContext.RequestAborted);

        if (userAccount is null)
        {
            return TypedResults.Forbid();
        }

        var currentOwner = httpContext.RequestServices.GetRequiredService<CurrentOwnerContext>();
        currentOwner.SetOwnerId(userAccount.OwnerId.Value);

        return await next(context);
    }
}
