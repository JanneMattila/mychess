using System.Threading.Tasks;
using MyChess.Handlers;

namespace MyChess.Tests.Handlers.Stubs
{
    public class NotificationHandlerStub : INotificationHandler
    {
        public async Task SendNotificationAsync(string userID, string gameID, string comment)
        {
            await Task.CompletedTask;
        }
    }
}
