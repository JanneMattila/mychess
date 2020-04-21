using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using MyChess.Functions.Interfaces;

namespace MyChess.Functions
{
    public static class GamesFunction
    {
        [FunctionName("Games")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            using var scope = log.BeginScope("Games");
            log.LogInformation("Games function processing request.");

            var games = new List<Game>();
            for (int i = 0; i < 5; i++)
            {
                games.Add(new Game()
                {
                    ID = Guid.NewGuid().ToString("B"),
                    Name = $"Game of name {i + 1}",
                    Opponent = $"User {i + 1}",
                    Updated = DateTimeOffset.UtcNow.AddHours(-i)
                });
            }

            return new OkObjectResult(games);
        }
    }
}
