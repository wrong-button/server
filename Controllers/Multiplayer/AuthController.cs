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
            var token = this.authToken.Sign(request.Player, request.RoomId);
            return Ok(new AuthResponse { Token = token });
        }
    }

    public record AuthRequest
    {
        [Required]
        public PlayerData Player { get; init; } = new();

        [Required]
        public string RoomId { get; init; } = "";
    }

    public record AuthResponse
    {
        public string Token { get; init; } = "";
    }
}
