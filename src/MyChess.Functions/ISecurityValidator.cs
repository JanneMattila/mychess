using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyChess.Functions
{
    public interface ISecurityValidator
    {
        Task<bool> InitializeAsync();
        Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(HttpRequest req);
    }
}
