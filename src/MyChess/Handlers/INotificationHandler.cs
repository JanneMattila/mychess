using System.Threading.Tasks;

namespace MyChess.Handlers
{
    public interface INotificationHandler
    {
        Task SendNotificationAsync(string userID, string gameID, string comment);
    }
}