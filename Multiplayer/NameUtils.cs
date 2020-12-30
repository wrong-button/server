using System;

namespace ExitPath.Server.Multiplayer
{
    public static class NameUtils
    {
        private static readonly Random rand = new Random();

        private static readonly string[] animals = new string[]
        {
            "Donkey",
            "Llama",
            "Platypus",
            "Hedgehog",
            "Giraffe",
            "Velociraptor",
            "Otter",
            "Hippo",
            "Elephant",
            "Kangaroo",
            "Marmot",
            "Falcon",
            "Tiger",
            "Cheetah",
            "Muskrat",
            "Deer",
            "Badger",
            "Lemur",
            "Possum",
            "Wildebeest",
            "Whale",
            "Dolphin",
            "Swordfish"
        };

        private static readonly string[] roadTypes = new string[]
        {
            "Rd.",
            "St.",
            "Blvd.",
            "Alley.",
            "Ave.",
            "Ct.",
            "Ln.",
            "Dr."
        };

        public static string GenerateRoomName()
        {
            lock (rand)
            {
                var animal = animals[rand.Next(animals.Length)];
                var roadType = roadTypes[rand.Next(roadTypes.Length)];
                return $"{rand.Next(10000)} {animal} {roadType}";
            }
        }
    }
}
