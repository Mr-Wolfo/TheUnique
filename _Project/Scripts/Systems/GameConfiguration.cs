using UnityEngine;

namespace TheUnique.Core
{
    public static class GameConfiguration
    {
        public static int WorldWidth = 64;
        public static int WorldHeight = 64;
        public static int Seed = 0;
        
        public static bool IsLoadRequest = false;
        
        public static void ResetToDefaults()
        {
            WorldWidth = 64;
            WorldHeight = 64;
            Seed = Random.Range(0, 999999);
            IsLoadRequest = false;
        }
    }
}