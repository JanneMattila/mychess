using System.Threading.Tasks;
using MyChess.Backend.Handlers;

namespace MyChess.Backend.Tests.Handlers.Stubs;

public class NotificationHandlerStub : INotificationHandler
{
    public async Task SendNotificationAsync(string userID, string gameID, string comment)
    {
        await Task.CompletedTask;
    }
}
