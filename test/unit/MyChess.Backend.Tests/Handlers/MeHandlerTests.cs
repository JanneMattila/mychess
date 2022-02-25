using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Backend.Tests.Handlers.Stubs;
using Xunit;

namespace MyChess.Backend.Tests.Handlers;

public class MeHandlerTests
{
    private readonly MeHandler _meHandler;
    private readonly MyChessContextStub _context;

    public MeHandlerTests()
    {
        _context = new MyChessContextStub();
        _meHandler = new MeHandler(NullLogger<MeHandler>.Instance, _context);
    }

    [Fact]
    public async Task Login()
    {
        // Arrange
        var expectedUserID = "user123";
        var user = new AuthenticatedUser()
        {
            UserIdentifier = "u",
            ProviderIdentifier = "p"
        };

        await _context.UpsertAsync(TableNames.Users, new UserEntity()
        {
            PartitionKey = "u",
            RowKey = "p",
            UserID = "user123"
        });

        // Act
        var actual = await _meHandler.LoginAsync(user);

        // Assert
        Assert.Equal(expectedUserID, actual.ID);
    }
}
