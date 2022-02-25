using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Functions.Tests.Helpers;
using MyChess.Functions.Tests.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Functions.Tests;

public class MeFunctionTests
{
    private readonly MeFunction _meFunction;
    private readonly SecurityValidatorStub _securityValidatorStub;
    private readonly MeHandlerStub _meHandlerStub;

    public MeFunctionTests()
    {
        _meHandlerStub = new MeHandlerStub();
        _securityValidatorStub = new SecurityValidatorStub();
        _meFunction = new MeFunction(NullLogger<MeFunction>.Instance, _meHandlerStub, _securityValidatorStub);
    }

    [Fact]
    public async Task No_ClaimsPrincipal_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Unauthorized;
        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _meFunction.Run(req);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
    }

    [Fact]
    public async Task No_Required_Permission_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Unauthorized;
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _meFunction.Run(req);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
    }

    [Fact]
    public async Task Fetch_Me_Test()
    {
        // Arrange
        var expected = HttpStatusCode.OK;
        var expectedUserID = "abc";

        _meHandlerStub.User.ID = "abc";

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _meFunction.Run(req);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
        actual.Body.Position = 0;
        var actualUser = await JsonSerializer.DeserializeAsync<User>(actual.Body);
        Assert.Equal(expectedUserID, actualUser?.ID);
    }
}
