using ExitPath.Server.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ExitPath.Server.Multiplayer
{
    public class AuthTokenService
    {
        private readonly JwtSecurityTokenHandler tokenHandler = new();
        private readonly AuthConfig config;
        private readonly SymmetricSecurityKey key;

        public AuthTokenService(IOptions<AuthConfig> config)
        {
            this.config = config.Value;
            this.key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.TokenSecret));
        }

        public string Sign(PlayerData data)
        {
            var desc = new SecurityTokenDescriptor
            {
                SigningCredentials = new SigningCredentials(this.key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = this.config.Authority,
                Audience = this.config.Authority,
                Claims = new Dictionary<string, object>
                {
                    { "sub", data.DisplayName },
                    { "player", data },
                }
            };
            return tokenHandler.CreateEncodedJwt(desc);
        }
    }
}
