using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Writer
{
    class Program
    {
        static void Main(string[] args)
        {

            for (int i=1; i < 5000; i++)
            {
                Console.WriteLine("This is message for stdout {0}", i);
            }

            for (int i = 1; i < 100; i++)
            {
                Console.Error.WriteLine("This is message for stderr {0}", i);
            }
            Console.Error.WriteLine("This is message for stderr");

        }
    }
}
