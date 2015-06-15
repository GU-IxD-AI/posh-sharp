using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using POSH.sys.parse;
using POSH.sys;

namespace POSH.testing.validate.sys.parse
{
    [TestFixture]
    class LapParserTest
    {
        LAPParser parser;
        
        string input = @"( 
                (C follow-player (minutes 10) (goal((fail)))
                    (elements
                      ((close-enough (trigger ((close-to-player))) stop-bot))
                      ((move (trigger ((see-player))) move-player))
                    )
                  )
                  (C wander-around (minutes 10) (goal((see-player)))
                    (elements
                      ((stuck (trigger ((is-stuck))) avoid))
                      ((pickup (trigger ((see-item))) pickup-item))
                      ((walk-around (trigger ((is-rotating 0))) walk))
                    )
                  )
                      
                  (AP avoid (minutes 10) (stop-bot rotate then-walk))
                      
                  (C then-walk (minutes 10) (goal((is-walking)))
                    (elements
                      ((try-walk (trigger ((is-rotating 0))) walk))
                    )
                  )
                      

                  (RDC life (goal ((fail)))
                      (drives
                        ((hit (trigger((hit-object)(is-rotating 0))) avoid))
                        ((follow (trigger((see-player))) follow-player))
                        ((wander (trigger((succeed))) wander-around))
                      )
                  )
                )";

        [SetUp]
        public void CreateLAPParser()
        {
            parser = new LAPParser();
        }

        [Test]
        public void RetrievePlanbuilder()
        {
            PlanBuilder build = parser.Parse(input);
            Console.Out.WriteLine("PlanBuilder object created: "+build.GetType());
        }

    
    }
}
