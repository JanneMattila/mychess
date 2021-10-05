using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Functions.Internal;
using MyChess.Interfaces;

namespace MyChess.Functions
{
    public class SettingsFunction
    {
        private readonly ILogger<SettingsFunction> _log;
        private readonly ISettingsHandler _settingsHandler;
        private readonly ISecurityValidator _securityValidator;

        public SettingsFunction(
            ILogger<SettingsFunction> log,
            ISettingsHandler settingsHandler,
            ISecurityValidator securityValidator)
        {
            _log = log;
            _settingsHandler = settingsHandler;
            _securityValidator = securityValidator;
        }

        [FunctionName("Settings")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "users/me/settings")] HttpRequest req)
        {
            using var _ = _log.FuncSettingsScope();
            _log.FuncSettingsStarted();

            var principal = await _securityValidator.GetClaimsPrincipalAsync(req);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            if (!principal.HasPermission(PermissionConstants.UserReadWrite))
            {
                _log.FuncSettingsUserDoesNotHavePermission(principal.Identity.Name, PermissionConstants.UserReadWrite);
                return new UnauthorizedResult();
            }

            var authenticatedUser = principal.ToAuthenticatedUser();

            _log.FuncSettingsProcessingMethod(req.Method);
            return req.Method switch
            {
                "GET" => await Get(authenticatedUser),
                "POST" => await PostAsync(authenticatedUser, req),
                _ => new StatusCodeResult((int)HttpStatusCode.NotImplemented)
            };
        }

        private async Task<IActionResult> Get(AuthenticatedUser authenticatedUser)
        {
            _log.FuncMeFetchMe();
            var settings = await _settingsHandler.GetSettingsAsync(authenticatedUser);
            return new OkObjectResult(settings);
        }

        private async Task<IActionResult> PostAsync(AuthenticatedUser authenticatedUser, HttpRequest req)
        {
            _log.FuncGamesCreateNewGame();
            var userSettings = await JsonSerializer.DeserializeAsync<UserSettings>(req.Body);
            var result = await _settingsHandler.UpdateSettingsAsync(authenticatedUser, userSettings);
            if (result == null)
            {
                return new OkResult();
            }
            else
            {
                var problemDetail = new ProblemDetails
                {
                    Detail = result.Detail,
                    Instance = result.Instance,
                    Status = result.Status,
                    Title = result.Title
                };

                return new ObjectResult(problemDetail)
                {
                    StatusCode = problemDetail.Status
                };
            }
        }
    }
}
