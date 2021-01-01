using System;
using System.ComponentModel.DataAnnotations;

namespace ExitPath.Server.Models
{
    public record PlayerData
    {
        [Required, StringLength(maximumLength: 40, MinimumLength = 3), RegularExpression("^[0-9a-zA-Z_-]+$")]
        public string DisplayName { get; init; } = "";

        [Range(0, 0xFFFFFF)]
        public int PrimaryColor { get; init; } = 0x33CCFF;

        [Range(0, 0xFFFFFF)]
        public int SecondaryColor { get; init; } = 0xFFFFFF;

        [Range(1, 24)]
        public int HeadType { get; init; } = 1;

        [Range(1, 24)]
        public int HandType { get; init; } = 1;

        [Range(0, double.PositiveInfinity)]
        public long XP { get; init; } = 0;

        [Range(0, double.PositiveInfinity)]
        public long Kudos { get; init; } = 0;

        [Range(0, double.PositiveInfinity)]
        public long Matches { get; init; } = 0;

        [Range(0, double.PositiveInfinity)]
        public long Wins { get; init; } = 0;
    }
}
