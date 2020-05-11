using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyChess.Functions.Tests.Stubs
{
    public class SecurityValidatorStub : ISecurityValidator
    {
        public ClaimsPrincipal ClaimsPrincipal { get; set; }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(HttpRequest req, ILogger log)
        {
            return await Task.FromResult<ClaimsPrincipal>(ClaimsPrincipal);
        }
    }
}
