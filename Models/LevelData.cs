using System;
using System.Text.RegularExpressions;

namespace ExitPath.Server.Models
{
    public record LevelData
    {
        private static readonly Regex v0Regex = new Regex("^(.+)#([a-zA-Z0-9+/]+=*)$");
        private static readonly Regex v1Regex = new Regex("^(.+)~#-([a-zA-Z0-9+/]+=*)~$");

        public string Name { get; set; } = "";


        public static LevelData Parse(string code)
        {
            string name;
            Match match;
            if ((match = v0Regex.Match(code)).Success)
            {
                name = match.Groups[1].Value;
            }
            else if ((match = v1Regex.Match(code)).Success)
            {
                name = match.Groups[1].Value;
            }
            else
            {
                throw new Exception("Invalid level code");
            }

            return new LevelData { Name = name };
        }
    }
}
