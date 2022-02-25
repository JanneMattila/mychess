using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Functions.Tests.Helpers;
using MyChess.Functions.Tests.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Functions.Tests;

public class FriendsFunctionTests
{
    private readonly FriendsFunction _friendsFunction;
    private readonly SecurityValidatorStub _securityValidatorStub;
    private readonly FriendsHandlerStub _friendsHandlerStub;

    public FriendsFunctionTests()
    {
        _friendsHandlerStub = new FriendsHandlerStub();
        _securityValidatorStub = new SecurityValidatorStub();
        _friendsFunction = new FriendsFunction(NullLogger<FriendsFunction>.Instance, _friendsHandlerStub, _securityValidatorStub);
    }

    [Fact]
    public async Task No_ClaimsPrincipal_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Unauthorized;
        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _friendsFunction.Run(req, string.Empty);

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
        var actual = await _friendsFunction.Run(req, string.Empty);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
    }

    [Fact]
    public async Task Fetch_Players_Test()
    {
        // Arrange
        var expected = HttpStatusCode.OK;
        var expectedFriends = 2;

        _friendsHandlerStub.Friends.Add(new User());
        _friendsHandlerStub.Friends.Add(new User());

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _friendsFunction.Run(req, string.Empty);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
        actual.Body.Position = 0;
        var list = await JsonSerializer.DeserializeAsync<List<User>>(actual.Body);
        Assert.Equal(expectedFriends, list?.Count);
    }

    [Fact]
    public async Task Fetch_Single_friend_Test()
    {
        // Arrange
        var expected = HttpStatusCode.OK;
        var expectedPlayerID = "abc";

        _friendsHandlerStub.SingleFriend = new User()
        {
            ID = "abc"
        };

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create();

        // Act
        var actual = await _friendsFunction.Run(req, "abc");

        // Assert
        Assert.Equal(expected, actual.StatusCode);
        actual.Body.Position = 0;
        var actualPlayer = await JsonSerializer.DeserializeAsync<User>(actual.Body);
        Assert.Equal(expectedPlayerID, actualPlayer?.ID);
    }

    [Fact]
    public async Task Add_Friend_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Created;
        var expectedPlayerID = "abc";
        var friend = new User()
        {
            ID = "abc",
            Name = "John Doe"
        };

        _friendsHandlerStub.SingleFriend = new User()
        {
            ID = "abc"
        };

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create("POST", body: friend);

        // Act
        var actual = await _friendsFunction.Run(req, "abc");

        // Assert
        Assert.Equal(expected, actual.StatusCode);
        actual.Body.Position = 0;
        var actualPlayer = await JsonSerializer.DeserializeAsync<User>(actual.Body);
        Assert.Equal(expectedPlayerID, actualPlayer?.ID);
    }

    [Fact]
    public async Task Add_Friend_Fails_Due_Invalid_Player_Test()
    {
        // Arrange
        var expectedError = "1234";
        var friend = new User()
        {
            ID = "abc",
            Name = "John Doe"
        };

        _friendsHandlerStub.Error = new HandlerError()
        {
            Instance = "some text/1234"
        };

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create("POST", body: friend);

        // Act
        var actual = await _friendsFunction.Run(req, "abc");

        // Assert
        actual.Body.Position = 0;
        var actualError = await JsonSerializer.DeserializeAsync<ProblemDetails>(actual.Body);
        Assert.EndsWith(expectedError, actualError?.Instance);
    }
}
