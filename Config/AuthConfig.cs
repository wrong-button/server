using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ExitPath.Server.Config
{
    public class AuthConfig
    {
        [Required, MinLength(32)]
        public string TokenSecret { get; set; } = "";

        [Required]
        public string Authority { get; set; } = "";

        public SigningCredentials CreateCredentials()
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.TokenSecret));
            return new SigningCredentials(key, "HS256");
        }
    }
}
