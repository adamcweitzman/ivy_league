using System;
using System.Collections.Generic;

namespace ivy_league
{
    public static class MyExtensions
    {
        private static Random rng = new Random();

        public static int WordCount(this String str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static T GetRandomFromList<T>(this IList<T> list)
        {
            var random = new Random();
            var index = random.Next(list.Count);

            return list[index];
        }
    }
}
