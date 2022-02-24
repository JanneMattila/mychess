using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace MyChess.Functions;

public class GameHub : ServerlessHub
{
    private readonly ILogger<GameHub> _log;
    private readonly ISecurityValidator _securityValidator;

    public GameHub(
        ILogger<GameHub> log,
        ISecurityValidator securityValidator)
    {
        _log = log;
        _securityValidator = securityValidator;
    }

    [FunctionName("negotiate")]
    public async Task<HttpResponseData> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequestData req)
    {
        var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
        if (principal == null)
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var authenticatedUser = principal.ToAuthenticatedUser();

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(Negotiate(authenticatedUser.UserIdentifier));
        return response;
    }
}
