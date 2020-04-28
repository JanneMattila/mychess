using System.Security.Claims;

namespace MyChess.Functions
{
    public static class ClaimsPrincipalExtensions
    {
        private const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";

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
    }
}
