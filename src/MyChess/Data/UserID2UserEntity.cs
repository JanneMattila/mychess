using Microsoft.Azure.Cosmos.Table;

namespace MyChess.Data
{
    public class UserID2UserEntity : TableEntity
    {
        public UserID2UserEntity()
        {
        }

        public string UserPrimaryKey { get; set; } = string.Empty;

        public string UserRowKey { get; set; } = string.Empty;
    }
}
