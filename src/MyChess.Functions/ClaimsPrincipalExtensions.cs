using System.Security.Claims;
using MyChess.Backend.Models;

namespace MyChess.Functions;

public static class ClaimsPrincipalExtensions
{
    private const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
    private const string ObjectIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
    private const string TenantIdentifierClaimType = "http://schemas.microsoft.com/identity/claims/tenantid";
    private const string NameClaimType = "name";
    private const string PreferredUsernameClaimType = "preferred_username";

    public static bool HasPermission(this ClaimsPrincipal principal, string requiredScope)
    {
        var scopeClaimValue = principal.FindFirstValue(ScopeClaimType);
        if (string.IsNullOrEmpty(scopeClaimValue))
        {
            // Does not contain required claim type
            return false;
        }

        var scopes = scopeClaimValue.Split(' ');
        foreach (var scope in scopes)
        {
            if (scope == requiredScope)
            {
                // Contains required permission
                return true;
            }
        }

        return false;
    }

    public static AuthenticatedUser ToAuthenticatedUser(this ClaimsPrincipal principal)
    {
        return new AuthenticatedUser()
        {
            UserIdentifier = principal.FindFirstValue(ObjectIdentifierClaimType) ?? string.Empty,
            ProviderIdentifier = principal.FindFirstValue(TenantIdentifierClaimType) ?? string.Empty,
            Name = principal.FindFirstValue(NameClaimType) ?? string.Empty,
            PreferredUsername = principal.FindFirstValue(PreferredUsernameClaimType) ?? string.Empty
        };
    }
}
