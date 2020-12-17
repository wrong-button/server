using System.ComponentModel.DataAnnotations;

namespace ExitPath.Server.Multiplayer
{
    public class AuthConfig
    {
        [Required, MinLength(32)]
        public string TokenSecret { get; set; } = "";

        [Required]
        public string Authority { get; set; } = "";
    }
}
