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

public class SettingsFunctionTests
{
    private readonly SettingsFunction _settingsFunction;
    private readonly SecurityValidatorStub _securityValidatorStub;
    private readonly SettingsHandlerStub _settingsHandlerStub;

    public SettingsFunctionTests()
    {
        _settingsHandlerStub = new SettingsHandlerStub();
        _securityValidatorStub = new SecurityValidatorStub();
        _settingsFunction = new SettingsFunction(NullLogger<SettingsFunction>.Instance, _settingsHandlerStub, _securityValidatorStub);
    }

    [Fact]
    public async Task No_ClaimsPrincipal_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Unauthorized;

        // Act
        var actual = await _settingsFunction.Run(null);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
    }

    [Fact]
    public async Task No_Required_Permission_Test()
    {
        // Arrange
        var expected = HttpStatusCode.Unauthorized;
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var actual = await _settingsFunction.Run(null);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
    }

    [Fact]
    public async Task Fetch_Settings_Test()
    {
        // Arrange
        var expected = HttpStatusCode.OK;
        var expectedPlayAlwaysUp = true;

        _settingsHandlerStub.UserSettings.PlayAlwaysUp = true;

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create("GET");

        // Act
        var actual = await _settingsFunction.Run(req);

        // Assert
        Assert.Equal(expected, actual.StatusCode);
        var actualUserSettings = await JsonSerializer.DeserializeAsync<UserSettings>(actual.Body);
        Assert.Equal(expectedPlayAlwaysUp, actualUserSettings?.PlayAlwaysUp);
    }

    [Fact]
    public async Task Update_Settings_Test()
    {
        // Arrange
        var expected = typeof(OkResult);

        var userSettings = new UserSettings();

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create("POST", body: userSettings);

        // Act
        var actual = await _settingsFunction.Run(req);

        // Assert
        var actualSettings = await JsonSerializer.DeserializeAsync<UserSettings>(actual.Body);
        Assert.IsType(expected, actualSettings);
    }


    [Fact]
    public async Task Update_Settings_With_Failure_Test()
    {
        // Arrange
        var expected = typeof(ObjectResult);

        var userSettings = new UserSettings();
        _settingsHandlerStub.Error = new HandlerError()
        {
            Status = 501
        };

        var identity = new ClaimsIdentity();
        identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", "User.ReadWrite"));
        _securityValidatorStub.ClaimsPrincipal = new ClaimsPrincipal(identity);

        var req = HttpRequestHelper.Create("POST", body: userSettings);

        // Act
        var actual = await _settingsFunction.Run(req);

        // Assert
        Assert.IsType(expected, actual);
    }
}
