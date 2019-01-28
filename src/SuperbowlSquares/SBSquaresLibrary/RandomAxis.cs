using System;
using System.Collections.Generic;
using System.Linq;

namespace SBSquaresLibrary
{
    public static class RandomAxis
    {
        private static int _length = 10;

        public static IEnumerable<int> generateAxis()
        {

            IEnumerable<int> axis = Enumerable.Range(0, _length);
            IEnumerable<int> scrambledAxis = axis.Shuffle();

            return (scrambledAxis);

        }

        private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new Random());
        }

        private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (rng == null) throw new ArgumentNullException("rng");

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(
            this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (int i = 0; i < buffer.Count; i++)
            {
                int j = rng.Next(i, buffer.Count);
                yield return buffer[j];

                buffer[j] = buffer[i];
            }
        }

    }
}
