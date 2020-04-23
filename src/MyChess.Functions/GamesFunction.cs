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

            var games = new List<GameHeader>();
            for (int i = 0; i < 5; i++)
            {
                games.Add(new GameHeader()
                {
                    ID = Guid.NewGuid().ToString("D"),
                    Name = $"Game of name {i + 1}",
                    Opponent = $"User {i + 1}",
                    Comment = "👍 Lorem ipsum dolor sit amet ❤ 😊, consectetur adipiscing elit. Ut sed mollis neque. Maecenas molestie nibh id elit gravida, quis placerat magna tempor. Morbi posuere orci sapien, eget dictum ligula tempor ut. Vivamus nec massa dolor. Sed fermentum ex non nunc dapibus blandit. Vivamus sollicitudin, libero rhoncus faucibus ullamcorper, velit dui finibus neque, sed placerat sapien urna id nunc. Aliquam ac consectetur elit.",
                    Updated = DateTimeOffset.UtcNow.AddHours(-i)
                });
            }

            return new OkObjectResult(games);
        }
    }
}
