using System.Threading.Tasks;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Handlers
{
    public interface ISettingsHandler
    {
        Task<UserSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser);
        Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, UserSettings playerSettings);
    }
}
