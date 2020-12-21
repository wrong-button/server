using ExitPath.Server.Config;
using ExitPath.Server.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace ExitPath.Server.Multiplayer
{
    public class AuthTokenService
    {
        private readonly JwtSecurityTokenHandler tokenHandler = new();
        private readonly AuthConfig config;
        private readonly SigningCredentials credentials;

        public AuthTokenService(IOptions<AuthConfig> config)
        {
            this.config = config.Value;
            this.credentials = config.Value.CreateCredentials();
        }

        public string Sign(PlayerData data, string roomId)
        {
            var desc = new SecurityTokenDescriptor
            {
                SigningCredentials = this.credentials,
                Issuer = this.config.Authority,
                Audience = this.config.Authority,
                Claims = new Dictionary<string, object>
                {
                    { "sub", data.DisplayName },
                    { "player", data },
                    { "roomId", roomId }
                }
            };
            return tokenHandler.CreateEncodedJwt(desc);
        }
    }
}
