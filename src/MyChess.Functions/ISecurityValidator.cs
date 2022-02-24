using System.Security.Claims;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions;

public interface ISecurityValidator
{
    Task<bool> InitializeAsync();
    Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(HttpRequestData req);
}
