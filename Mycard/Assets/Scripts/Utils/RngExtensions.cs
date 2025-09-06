using System.Collections.Generic;

namespace Game.Utils
{
    /// <summary>
    /// IRngService를 위한 유용한 확장 메서드를 제공합니다.
    /// </summary>
    public static class RngExtensions
    {
        /// <summary>
        /// Fisher–Yates 알고리즘으로 리스트를 제자리에서 섞습니다.
        /// </summary>
        public static void Shuffle<T>(this IRngService rng, string domain, IList<T> list)
        {
            if (list == null || list.Count <= 1) return;
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(domain, 0, i + 1);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }
    }
}

