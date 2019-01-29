using System.Collections.Generic;
using System.Linq;

namespace SBSquaresLibrary
{
    public class RandomAxis
    {
        private static int _length = 10;

        public IEnumerable<int> GenerateAxis()
        {
            IEnumerable<int> axis = Enumerable.Range(0, _length);
            IEnumerable<int> scrambledAxis = axis.Shuffle();

            return scrambledAxis;
        }
    }
}