using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Handlers;
using MyChess.Interfaces;

namespace MyChess.Functions
{
    public class GamesMoveFunction
    {
        private readonly IGamesHandler _gamesHandler;
        private readonly ISecurityValidator _securityValidator;

        public GamesMoveFunction(
            IGamesHandler gamesHandler,
            ISecurityValidator securityValidator)
        {
            _gamesHandler = gamesHandler;
            _securityValidator = securityValidator;
        }

        [FunctionName("GamesMove")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "games/{id?}/moves")] HttpRequest req,
            string id,
            ILogger log)
        {
            using var scope = log.BeginScope("GamesMove");
            log.LogInformation(LoggingEvents.FuncGamesMoveStarted, "GamesMove function processing request.");

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.GamesReadWrite))
            {
                log.LogWarning(LoggingEvents.FuncGamesMoveUserDoesNotHavePermission,
                    "User {user} does not have permission {permission}", principal.Identity.Name, PermissionConstants.GamesReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            log.LogInformation(LoggingEvents.FuncGamesMoveProcessingMethod,
                "Processing {method} request", req.Method);

            var moveToAdd = await JsonSerializer.DeserializeAsync<MyChessGameMove>(req.Body);
            var error = await _gamesHandler.AddMoveAsync(authenticatedUser, id, moveToAdd);
            if (error == null)
            {
                return new OkResult();
            }
            else
            {
                var problemDetail = new ProblemDetails
                {
                    Detail = error.Detail,
                    Instance = error.Instance,
                    Status = error.Status,
                    Title = error.Title
                };

                return new ObjectResult(problemDetail)
                {
                    StatusCode = problemDetail.Status
                };
            }
        }
    }
}
