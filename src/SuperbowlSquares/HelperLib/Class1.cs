using System;

namespace HelperLib
{
    public class Class1
    {
        public double GetNumber()
        {
            Random rand = new Random();
            return rand.NextDouble();
        }
    }
}
