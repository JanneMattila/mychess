using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace MyChess.Functions
{
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
        public async Task<IActionResult> Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        {
            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            return new OkObjectResult(Negotiate(authenticatedUser.UserIdentifier));
        }
    }
}
