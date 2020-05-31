using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MyChess.Functions.Tests.Helpers;
using Xunit;

namespace MyChess.Functions.Tests
{
    public class SecurityValidatorTests
    {
        private readonly SecurityValidator _securityValidator;

        public SecurityValidatorTests()
        {
            var options = new AzureADOptions();
            _securityValidator = new SecurityValidator(NullLogger<SecurityValidator>.Instance, Options.Create(options));
        }

        [Theory]
        [InlineData("", "", null)]
        [InlineData("Authorization", "", null)]
        [InlineData("Authorization", "Bearer", null)]
        [InlineData("Authorization", "Basic abcdef", null)]
        [InlineData("Authorization", "Bearer abcdef", "abcdef")]
        public void Security_Validator_Header_Test(string name, string value, string? expected)
        {
            // Arrange
            var request = HttpRequestHelper.Create("GET", headers: new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>()
            {
                {  name, value }
            });

            // Act
            var actual = _securityValidator.ParseAccessToken(request);

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
