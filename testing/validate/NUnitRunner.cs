using System;
using NUnit;

namespace POSH_sharp.testing.validate
{
    class NUnitRunner
    {
        [STAThread]
        static void Main(string[] args)
        {
            NUnit.ConsoleRunner.Runner.Main(args);
        }
    }
}
