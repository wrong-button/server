using ExitPath.Server.Models;
using ExitPath.Server.Multiplayer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace ExitPath.Server.Controllers.Multiplayer
{
    [ApiController]
    [Route("api/multiplayer/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthTokenService authToken;

        public AuthController(AuthTokenService authToken)
        {
            this.authToken = authToken;
        }

        public IActionResult Handle([FromBody] AuthRequest request)
        {
            if (request.Version < MultiplayerHub.ProtocolVersion)
            {
                return BadRequest(new { Error = "Old version, please reload and try again." });
            }
            var token = this.authToken.Sign(request.Player);
            return Ok(new AuthResponse { Token = token });
        }
    }

    public record AuthRequest
    {
        [Required]
        public PlayerData Player { get; init; } = new();

        public int Version { get; init; } = 0;
    }

    public record AuthResponse
    {
        public string Token { get; init; } = "";
    }
}
