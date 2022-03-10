using System.Threading.Tasks;

namespace MyChess.Backend.Handlers;

public interface INotificationHandler
{
    Task SendNotificationAsync(string userID, string gameID, string comment);
}
