﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using MyChess.Backend.Data;
using MyChess.Backend.Handlers;
using MyChess.Backend.Models;
using MyChess.Backend.Tests.Handlers.Stubs;
using MyChess.Interfaces;
using Xunit;

namespace MyChess.Backend.Tests.Handlers;

public class FriendsHandlerTests
{
    private readonly FriendsHandler _friendsHandler;
    private readonly MyChessContextStub _context;

    public FriendsHandlerTests()
    {
        _context = new MyChessContextStub();
        _friendsHandler = new FriendsHandler(NullLogger<FriendsHandler>.Instance, _context);
    }

    [Fact]
    public async Task Get_Friends_As_New_User()
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
        var actual = await _friendsHandler.GetFriendsAsync(user);

        // Assert
        Assert.Equal(expected, actual.Count);
    }

    [Fact]
    public async Task Get_Friends_As_Existing_User_With_Friend()
    {
        // Arrange
        var expectedID = "123";
        var expectedName = "My Friend";
        var user = new AuthenticatedUser()
        {
            Name = "abc",
            PreferredUsername = "a b",
            UserIdentifier = "u",
            ProviderIdentifier = "p"
        };

        await _context.UpsertAsync(TableNames.Users, new UserEntity()
        {
            PartitionKey = "u",
            RowKey = "p",
            UserID = "user123"
        });
        await _context.UpsertAsync(TableNames.UserFriends, new UserFriendEntity()
        {
            PartitionKey = "user123",
            RowKey = "123",
            Name = "My Friend"
        });

        // Act
        var actual = await _friendsHandler.GetFriendAsync(user, "123");

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expectedID, actual?.ID);
        Assert.Equal(expectedName, actual?.Name);
    }

    [Fact]
    public async Task Add_Friend_But_No_Player_Found()
    {
        // Arrange
        var expected = "2203"; // FriendHandlerPlayerNotFound
        var user = new AuthenticatedUser()
        {
            Name = "abc",
            PreferredUsername = "a b",
            UserIdentifier = "u",
            ProviderIdentifier = "p"
        };

        var friendToAdd = new User();

        // Act
        var actual = await _friendsHandler.AddNewFriend(user, friendToAdd);

        // Assert
        Assert.Null(actual.Friend);
        Assert.NotNull(actual.Error);
        Assert.EndsWith(expected, actual.Error?.Instance);
    }

    [Fact]
    public async Task Add_Friend()
    {
        // Arrange
        var expectedID = "user456";
        var expectedName = "My Best Friend";

        var user = new AuthenticatedUser()
        {
            Name = "abc",
            PreferredUsername = "a b",
            UserIdentifier = "u1",
            ProviderIdentifier = "p1"
        };

        // Player adding the friend
        await _context.UpsertAsync(TableNames.Users, new UserEntity()
        {
            PartitionKey = "u1",
            RowKey = "p1",
            UserID = "user123",
            Name = "My Name"
        });

        // Friend
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
        var friendToAdd = new User()
        {
            ID = "user456",
            Name = "My Best Friend"
        };

        // Act
        var actual = await _friendsHandler.AddNewFriend(user, friendToAdd);

        // Assert
        Assert.Null(actual.Error);
        Assert.NotNull(actual.Friend);
        Assert.Equal(expectedID, actual.Friend?.ID);
        Assert.Equal(expectedName, actual.Friend?.Name);

        var friend1 = await _context.GetAsync<UserFriendEntity>(TableNames.UserFriends, "user123", "user456");
        Assert.NotNull(friend1);
        Assert.Equal("My Best Friend", friend1?.Name);

        var friend2 = await _context.GetAsync<UserFriendEntity>(TableNames.UserFriends, "user456", "user123");
        Assert.NotNull(friend2);
        Assert.Equal("My Name", friend2?.Name);
    }
}
