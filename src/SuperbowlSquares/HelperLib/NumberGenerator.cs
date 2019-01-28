using System;
using System.Collections.Generic;
using System.Linq;

namespace HelperLib
{
    public class NumberGenerator
    {
        public IEnumerable<int> GetTenNumbers()
        {
            return Enumerable.Range(1, 10);
        }
    }
}