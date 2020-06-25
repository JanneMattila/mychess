using System.Threading.Tasks;
using MyChess.Handlers;
using MyChess.Interfaces;
using MyChess.Models;

namespace MyChess.Functions.Tests.Stubs
{
    public class SettingsHandlerStub : ISettingsHandler
    {
        public UserSettings UserSettings { get; set; } = new UserSettings();

        public HandlerError? Error { get; set; }

        public async Task<UserSettings> GetSettingsAsync(AuthenticatedUser authenticatedUser)
        {
            return await Task.FromResult(UserSettings);
        }

        public async Task<HandlerError?> UpdateSettingsAsync(AuthenticatedUser authenticatedUser, UserSettings playerSettings)
        {
            return await Task.FromResult(Error);
        }
    }
}
