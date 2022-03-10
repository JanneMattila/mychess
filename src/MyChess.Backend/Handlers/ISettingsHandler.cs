using System.Threading.Tasks;
using MyChess.Backend.Models;
using MyChess.Interfaces;

namespace MyChess.Backend.Handlers;

public interface ISettingsHandler
{
    Task<UserSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser);
    Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, UserSettings userSettings);
}
