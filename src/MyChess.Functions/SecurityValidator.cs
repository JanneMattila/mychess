using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using MyChess.Internal;

namespace MyChess.Functions
{
    public class SecurityValidator : ISecurityValidator
    {
        private readonly ILogger<SecurityValidator> _log;
        private readonly AzureADOptions _securityValidatorOptions;
        private readonly SemaphoreSlim _initializationSemaphore = new SemaphoreSlim(1, 1);
        private TokenValidationParameters? _tokenValidationParameters;

        public SecurityValidator(ILogger<SecurityValidator> log, IOptions<AzureADOptions> securityValidatorOptions)
        {
            _log = log;
            _securityValidatorOptions = securityValidatorOptions.Value;
        }

        public async Task<bool> InitializeAsync()
        {
            if (_tokenValidationParameters == null)
            {
                await _initializationSemaphore.WaitAsync();
                if (_tokenValidationParameters == null)
                {
                    try
                    {
                        _log.FuncSecInitializing();
                        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                            $"https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration",
                            new OpenIdConnectConfigurationRetriever());

                        var configuration = await configurationManager.GetConfigurationAsync();
                        _tokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKeys = configuration.SigningKeys,
                            ValidAudiences = new[]
                                {
                                    _securityValidatorOptions.Audience,
                                    _securityValidatorOptions.ClientId
                                },
                            ValidateIssuer = true,
                            IssuerValidator = (issuer, securityToken, validationParameters) =>
                            {
                                return IssuerValidationLogic(issuer) ? issuer : null;
                            }
                        };
                        _log.FuncSecInitialized();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.FuncSecInitializingFailed(ex);
                        return false;
                    }
                    finally
                    {
                        _initializationSemaphore.Release();
                    }
                }
            }
            return true;
        }

        private bool IssuerValidationLogic(string issuer)
        {
            const string start = "https://login.microsoftonline.com/";
            const string end = "/v2.0";
            if (!issuer.StartsWith(start) || !issuer.EndsWith(end))
            {
                _log.FuncSecInvalidIssuer(issuer);
                return false;
            }

            var tenant = issuer.Replace(start, string.Empty).Replace(end, string.Empty);
            _log.FuncSecIssuer(tenant);
            return true;
        }

        public string? ParseAccessToken(HttpRequest req)
        {
            if (!req.Headers.ContainsKey(HeaderNames.Authorization))
            {
                _log.FuncSecNoAuthHeader();
                return null;
            }

            var authorizationValue = req.Headers[HeaderNames.Authorization].ToString().Split(' ');
            if (authorizationValue.Length != 2 ||
                authorizationValue[0] != JwtBearerDefaults.AuthenticationScheme)
            {
                _log.FuncSecNoBearerToken();
                return null;
            }

            return authorizationValue[1];
        }

        public async Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(HttpRequest req)
        {
            var accessToken = ParseAccessToken(req);
            if (accessToken == null)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var success = await InitializeAsync();
            if (!success)
            {
                return null;
            }

            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(
                    accessToken, 
                    _tokenValidationParameters, 
                    out SecurityToken securityToken);
                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                _log.FuncSecTokenValidationFailed(ex);
                return null;
            }
        }
    }
}
