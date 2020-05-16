using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace MyChess.Functions.Tests.Stubs
{
    public class SecurityValidatorStub : ISecurityValidator
    {
        public ClaimsPrincipal ClaimsPrincipal { get; set; }

        public async Task<ClaimsPrincipal> GetClaimsPrincipalAsync(HttpRequest req)
        {
            return await Task.FromResult<ClaimsPrincipal>(ClaimsPrincipal);
        }
    }
}
