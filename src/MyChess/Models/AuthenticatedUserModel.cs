namespace MyChess.Interfaces
{
    public class AuthenticatedUserModel
    {
        public string UserIdentifier { get; set; } = string.Empty;

        public string ProviderIdentifier { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string PreferredUsername { get; set; } = string.Empty;
    }
}
