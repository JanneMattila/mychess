using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class UserFriendEntity : TableEntity
    {
        public UserFriendEntity()
        {
        }

        public string Name { get; set; } = string.Empty;
    }
}
