using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions.Tests.Stubs;

public class SecurityValidatorStub : ISecurityValidator
{
    public bool InitializationResult { get; set; } = true;
    public ClaimsPrincipal? ClaimsPrincipal { get; set; }

    public async Task<bool> InitializeAsync()
    {
        return await Task.FromResult(InitializationResult);
    }

    public async Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(HttpRequestData req)
    {
        return await Task.FromResult<ClaimsPrincipal>(ClaimsPrincipal);
    }
}
