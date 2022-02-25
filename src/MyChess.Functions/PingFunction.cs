using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace MyChess.Functions;

public class PingFunction
{
    private readonly ISecurityValidator _securityValidator;

    public PingFunction(ISecurityValidator securityValidator)
    {
        _securityValidator = securityValidator;
    }

    [Function("Ping")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ping")] HttpRequestData req)
    {
        await _securityValidator.InitializeAsync();
        return req.CreateResponse(HttpStatusCode.OK);
    }
}
