using System.Collections.Generic;
using UnityEngine;

namespace Game.Level
{
    public static class LevelSeeds
    {
        // Інше зерно лише для номера, який треба зібрати заново.
        // Решта рівнів лишається тією самою в усіх гравців.
        static readonly Dictionary<int, int> replaced = new Dictionary<int, int>
        {
        };

        public static int For(int levelId)
        {
            levelId = Mathf.Max(1, levelId);
            if (replaced.TryGetValue(levelId, out int custom))
                return custom;

            unchecked
            {
                int mixed = levelId * 374761393 + 668265263;
                mixed = (mixed ^ (mixed >> 13)) * 1274126177;
                return mixed == 0 ? 1 : mixed;
            }
        }
    }
}
