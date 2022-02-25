using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyChess.Functions.Tests.Helpers;
using Xunit;

namespace MyChess.Functions.Tests;

public class SecurityValidatorTests
{
    private readonly SecurityValidator _securityValidator;

    public SecurityValidatorTests()
    {
        var options = new AzureADOptions();
        _securityValidator = new SecurityValidator(NullLogger<SecurityValidator>.Instance, Options.Create(options));
    }

    [Theory]
    [InlineData("X-MyChessAuth", "", null)]
    [InlineData("X-MyChessAuth", "Bearer", null)]
    [InlineData("X-MyChessAuth", "Basic abcdef", null)]
    [InlineData("X-MyChessAuth", "Bearer abcdef", "abcdef")]
    public void Security_Validator_Header_Test(string name, string value, string? expected)
    {
        // Arrange
        var request = HttpRequestHelper.Create(headers: new HttpHeadersCollection()
        {
            {  name, value }
        });

        // Act
        var actual = _securityValidator.ParseAccessToken(request);

        // Assert
        Assert.Equal(expected, actual);
    }
}
