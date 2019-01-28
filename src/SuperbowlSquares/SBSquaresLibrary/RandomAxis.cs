using System.Collections.Generic;
using System.Linq;

namespace SBSquaresLibrary
{
    public class RandomAxis
    {
        private int _length = 10;
        
        public IEnumerable<int> GetUnscrambledAxis(int length = 10)
        {
            return Enumerable.Range(1, length);
        }
    }
}
