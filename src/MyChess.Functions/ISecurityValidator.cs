using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyChess.Functions
{
    public interface ISecurityValidator
    {
        Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(HttpRequest req);
    }
}
