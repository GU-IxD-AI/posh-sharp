using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POSH_sharp.test.examples.poshbot
{
    /// <summary>
    /// Gamebots Example
    /// 
    /// We need to start a comms thread in order to get updates
    /// to the agent status from the server.
    /// </summary>
    class World
    {
        // WARNING: This behaviour has not been updated with the latest refactoring of
        // the POSH implementation. So it won't work. (29/07/08)
        public World()
        {
        }

        public static void Main(string [] args){
        }
    }


}


//from __future__ import nested_scopes
//from socket import *
//from POSH.basic import Base
//from POSH import posh_utils
//import re
//import thread
//import random

//# Init world in this example connects to gamebots server
//def init_world(*args, **kw):
//    pass
    

//# Returns the behavior object
//def make_behavior(ip, port, botname, agent, *args, **kw):
//    bot = Bot_Agent(agent, ip, port, botname)
//    b = Behavior(agent = agent)
//    b.bind_bot(bot)
//    b.bot.connect()
//    return [b]

//# Called when pyposh is shutting down
//def destroy_world():
//    pass

//# Some utility functions
//def find_distance(one, two):
//    (x1, y1) = one
//    (x2, y2) = two
//    return ((((x1-x2)**2) + ((y1-y2)**2))**0.5)
