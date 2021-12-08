using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Backend.Tests.Handlers.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Backend.Tests.Handlers
{
    public class GamesHandlerTests
    {
        private readonly GamesHandler _gamesHandler;
        private readonly MyChessContextStub _context;
        private readonly NotificationHandlerStub _notificationHandler;

        public GamesHandlerTests()
        {
            _context = new MyChessContextStub();
            _notificationHandler = new NotificationHandlerStub();
            var chessBoard = new ChessBoard(NullLogger<ChessBoard>.Instance);
            _gamesHandler = new GamesHandler(NullLogger<GamesHandler>.Instance, _context, _notificationHandler, chessBoard);
        }

        [Fact]
        public async Task Get_Games_As_New_User_No_Games()
        {
            // Arrange
            var expected = 0;
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            // Act
            var actual = await _gamesHandler.GetGamesAsync(user, null);

            // Assert
            Assert.Equal(expected, actual.Count);
        }

        [Fact]
        public async Task Get_Games_As_Existing_User_With_Game()
        {
            // Arrange
            var expected = "123";
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            var compactor = new Compactor();
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u",
                RowKey = "p",
                UserID = "user123"
            });
            await _context.UpsertAsync(TableNames.GamesWaitingForYou, new GameEntity()
            {
                PartitionKey = "user123",
                RowKey = "123",
                Data = compactor.Compact(new MyChessGame() { ID = "123" })
            });

            // Act
            var actual = await _gamesHandler.GetGameAsync(user, "123", null);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(expected, actual?.ID);
        }

        [Fact]
        public async Task Create_New_Game_But_No_Opponent_Found()
        {
            // Arrange
            var expected = "2103"; // GameHandlerOpponentNotFound
            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u",
                ProviderIdentifier = "p"
            };

            var gameToCreate = new MyChessGame();

            // Act
            var actual = await _gamesHandler.CreateGameAsync(user, gameToCreate);

            // Assert
            Assert.Null(actual.Game);
            Assert.NotNull(actual.Error);
            Assert.EndsWith(expected, actual.Error?.Instance);
        }

        [Fact]
        public async Task Create_New_Game()
        {
            // Arrange
            var expectedWhitePlayer = "user123";
            var expectedBlackPlayer = "user456";

            var user = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u1",
                ProviderIdentifier = "p1"
            };

            // Player creating the game
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u1",
                RowKey = "p1",
                UserID = "user123"
            });

            // Opponent
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u2",
                RowKey = "p2",
                UserID = "user456"
            });
            await _context.UpsertAsync(TableNames.UserID2User, new UserID2UserEntity()
            {
                PartitionKey = "user456",
                RowKey = "user456",
                UserPrimaryKey = "u2",
                UserRowKey = "p2"
            });
            var gameToCreate = new MyChessGame();
            gameToCreate.Players.Black.ID = "user456";
            gameToCreate.Moves.Add(new MyChessGameMove());

            // Act
            var actual = await _gamesHandler.CreateGameAsync(user, gameToCreate);

            // Assert
            Assert.Null(actual.Error);
            Assert.NotNull(actual.Game);
            Assert.Equal(expectedWhitePlayer, actual.Game?.Players.White.ID);
            Assert.Equal(expectedBlackPlayer, actual.Game?.Players.Black.ID);
        }

        [Fact]
        public async Task Add_Move_To_Existing_Game()
        {
            // Arrange
            var expectedWaitingForYou = 1;
            var expectedWaitingForOpponent = 1;

            var user1 = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u1",
                ProviderIdentifier = "p1"
            };

            var user2 = new AuthenticatedUser()
            {
                Name = "def",
                PreferredUsername = "c d",
                UserIdentifier = "u2",
                ProviderIdentifier = "p2"
            };

            // Player creating the game
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u1",
                RowKey = "p1",
                UserID = "user123"
            });

            // Opponent
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u2",
                RowKey = "p2",
                UserID = "user456"
            });
            await _context.UpsertAsync(TableNames.UserID2User, new UserID2UserEntity()
            {
                PartitionKey = "user456",
                RowKey = "user456",
                UserPrimaryKey = "u2",
                UserRowKey = "p2"
            });
            var gameToCreate = new MyChessGame();
            gameToCreate.Players.Black.ID = "user456";
            gameToCreate.Moves.Add(new MyChessGameMove()
            {
                Move = "A2A3"
            });
            var createResponse = await _gamesHandler.CreateGameAsync(user1, gameToCreate);
            var gameID = createResponse.Game?.ID;
            var move = new MyChessGameMove()
            {
                Move = "A7A6"
            };

            // Act
            var actual = await _gamesHandler.AddMoveAsync(user2, gameID, move);

            // Assert
            Assert.Null(actual);
            var actualWaitingForYou = _context.Tables[TableNames.GamesWaitingForYou].Count;
            var actualWaitingForOpponent = _context.Tables[TableNames.GamesWaitingForOpponent].Count;
            Assert.Equal(expectedWaitingForYou, actualWaitingForYou);
            Assert.Equal(expectedWaitingForOpponent, actualWaitingForOpponent);
        }

        [Fact]
        public async Task Move_Game_To_Archive()
        {
            // Arrange
            var expectedWaitingForYou = 0;
            var expectedWaitingForOpponent = 0;
            var expectedArchive = 2;

            var user1 = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u1",
                ProviderIdentifier = "p1"
            };

            // Player creating the game
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u1",
                RowKey = "p1",
                UserID = "user123"
            });
            await _context.UpsertAsync(TableNames.UserID2User, new UserID2UserEntity()
            {
                PartitionKey = "user123",
                RowKey = "user123",
                UserPrimaryKey = "u1",
                UserRowKey = "p1"
            });

            // Opponent
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u2",
                RowKey = "p2",
                UserID = "user456"
            });

            var game = new MyChessGame
            {
                ID = "aaa"
            };
            game.Players.White.ID = "user123";
            game.Players.Black.ID = "user456";
            var moves = new string[]
            {
                "E2E4", // White Pawn
                "A7A6", // Black Pawn
                "F1C4", // White Bishop
                "H7H6", // Black Pawn
                "D1F3", // White Queen
                "A6A5"  // Black Pawn
            };

            foreach (var m in moves)
            {
                game.Moves.Add(new MyChessGameMove() { Move = m });
            }

            var compactor = new Compactor();
            var data = compactor.Compact(game);
            await _context.UpsertAsync(TableNames.GamesWaitingForYou, new GameEntity()
            {
                PartitionKey = "user123",
                RowKey = "aaa",
                Data = data
            });

            // White Queen
            var finalMove = new MyChessGameMove() { Move = "F3F7" };

            // Act
            var actual = await _gamesHandler.AddMoveAsync(user1, "aaa", finalMove);

            // Assert
            Assert.Null(actual);
            var actualWaitingForYou = _context.Tables[TableNames.GamesWaitingForYou].Count;
            var actualWaitingForOpponent = _context.Tables[TableNames.GamesWaitingForOpponent].Count;
            var actualWaitingArchive = _context.Tables[TableNames.GamesArchive].Count;
            Assert.Equal(expectedWaitingForYou, actualWaitingForYou);
            Assert.Equal(expectedWaitingForOpponent, actualWaitingForOpponent);
            Assert.Equal(expectedArchive, actualWaitingArchive);
        }

        [Fact]
        public async Task Delete_Archived_Game()
        {
            // Arrange
            var user1 = new AuthenticatedUser()
            {
                Name = "abc",
                PreferredUsername = "a b",
                UserIdentifier = "u1",
                ProviderIdentifier = "p1"
            };

            // Player creating the game
            await _context.UpsertAsync(TableNames.Users, new UserEntity()
            {
                PartitionKey = "u1",
                RowKey = "p1",
                UserID = "user123"
            });
            await _context.UpsertAsync(TableNames.UserID2User, new UserID2UserEntity()
            {
                PartitionKey = "user123",
                RowKey = "user123",
                UserPrimaryKey = "u1",
                UserRowKey = "p1"
            });

            var game = new MyChessGame
            {
                ID = "aaa"
            };
            game.State = ChessBoardState.CheckMate.ToString();
            game.Players.White.ID = "user123";

            var compactor = new Compactor();
            var data = compactor.Compact(game);
            await _context.UpsertAsync(TableNames.GamesArchive, new GameEntity()
            {
                PartitionKey = "user123",
                RowKey = "aaa",
                Data = data
            });

            // Act
            var actual = await _gamesHandler.DeleteGameAsync(user1, "aaa");

            // Assert
            Assert.NotNull(actual);
            Assert.IsType<HandlerError>(actual);
        }
    }
}
