using System;
using NUnit;

namespace POSH_sharp_Testing.validate
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
