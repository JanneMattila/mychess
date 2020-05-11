using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Handlers;

namespace MyChess.Functions
{
    public class GamesFunction
    {
        private readonly GamesHandler _gamesHandler;
        private readonly ISecurityValidator _securityValidator;

        public GamesFunction(
            GamesHandler gamesHandler,
            ISecurityValidator securityValidator)
        {
            _gamesHandler = gamesHandler;
            _securityValidator = securityValidator;
        }

        [FunctionName("Games")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using var scope = log.BeginScope("Games");
            log.LogInformation(LoggingEvents.FuncGamesStarted, "Games function processing request.");

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req, log);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
            {
                log.LogWarning(LoggingEvents.FuncGamesUserDoesNotHavePermission,
                    "User {user} does not have permission {permission}", principal.Identity.Name, PermissionConstants.GamesReadWrite);
                return new UnauthorizedResult();
            }

            var games = await _gamesHandler.GetGamesAsync(principal.ToAuthenticatedUser());
            return new OkObjectResult(games);
        }
    }
}
