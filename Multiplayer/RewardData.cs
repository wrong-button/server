namespace ExitPath.Server.Multiplayer
{
    public static class RewardData
    {
        public static int MatchXP(int rank) => rank switch
        {
            0 => 250,
            1 => 150,
            2 => 100,
            _ => 25,
        };

        public static int MatchKudos(int rank) => rank switch
        {
            0 => 3,
            1 => 2,
            2 => 1,
            _ => 0,
        };

        public const int MaxLevel = 41;

        public static int LevelXP(long level) => level switch
        {
            0 => 0,
            1 => 125,
            2 => 375,
            3 => 750,
            4 => 1250,
            5 => 1875,
            6 => 2625,
            7 => 3500,
            8 => 4500,
            9 => 5625,
            10 => 6875,
            11 => 8250,
            12 => 9750,
            13 => 11375,
            14 => 13125,
            15 => 15000,
            16 => 17000,
            17 => 19125,
            18 => 21375,
            19 => 23750,
            20 => 26250,
            21 => 28875,
            22 => 31625,
            23 => 34500,
            24 => 37500,
            25 => 40625,
            26 => 43875,
            27 => 47250,
            28 => 50750,
            29 => 54375,
            30 => 58125,
            31 => 62000,
            32 => 66000,
            33 => 70125,
            34 => 74375,
            35 => 78750,
            36 => 83250,
            37 => 87875,
            38 => 92625,
            39 => 97500,
            _ => 102500
        };

        public static int XPLevel(long xp)
        {
            int level;
            for (level = 1; level <= MaxLevel; level++)
            {
                if (xp < LevelXP(level))
                {
                    break;
                }
            }
            return level - 1;
        }

        public static string LevelName(int level) => level switch
        {
            0 => "Novice",
            1 => "Walker",
            2 => "Jogger",
            3 => "Runner",
            4 => "Sprinter",
            5 => "Windwalker",
            6 => "Speedrunner",
            7 => "Marathoner",
            8 => "Earthrunner",
            9 => "Pacer",
            10 => "Drifter",
            11 => "Voyager",
            12 => "Nimble",
            13 => "Expeditive",
            14 => "Quick",
            15 => "Outlast",
            16 => "Spring",
            17 => "Survivor",
            18 => "Swift",
            19 => "Trekker",
            20 => "Tour",
            21 => "Grace",
            22 => "Agility",
            23 => "Fast",
            24 => "Zoom",
            25 => "Fervor",
            26 => "Rush",
            27 => "Fly",
            28 => "Momentum",
            29 => "Haste",
            30 => "Fleetness",
            31 => "Forge",
            32 => "Drive",
            33 => "Boost",
            34 => "Velocity",
            35 => "Swiftness",
            36 => "Accelerator",
            37 => "Impel",
            38 => "Breakneck",
            39 => "Run Master",
            _ => "Grandmaster"
        };
    }
}
