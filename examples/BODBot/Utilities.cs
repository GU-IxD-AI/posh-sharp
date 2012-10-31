using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POSH_sharp.sys;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.examples.BODBot
{
    /// <summary>
    /// Some utility functions
    /// </summary>
    public static class Utilities
    {




        ///// <summary>
        ///// returns negative if the number a represents is < the number b represents.  0 if equal, positive if >
        ///// </summary>
        ///// <param name="one">int number one</param>
        ///// <param name="two">int number two</param>
        ///// <returns>-1 if one less than two 
        /////     0 if one equals two
        /////     1 if a is bigger than two</returns>
        //public int CompareNumberStrings(string one, string two)
        //{
        //    int a = int.Parse(one);
        //    int b = int.Parse(two);

        //    if (a == b)
        //        return 0;
        //    if (a < b)
        //        return -1;
        //    else 
        //        return 1;
        //}

        // now in NavPoint
        ///// <summary>
        ///// lists of nav points arrive as dicts with an "ID" key and keys "0", "1", .... "n" these need converting to lists
        ///// </summary>
        ///// <param name="?"></param>
        ///// <returns></returns>
        //public NavPoint[] GetOrderedNavPoints(Dictionary<string,NavPoint> navPoints)
        //{
        //    // remove the ID key to leave just numbers
        //    dictionary.Remove("ID");
        //    List<Vector3> locations = new List<Vector3>();
            
        //    // now get a list of just keys, and sort it to use in extracting the key:value pairs
        //    Dictionary<string,Vector3>.KeyCollection keyList = dictionary.Keys;

        //    // debug
        //    if (dictionary.ContainsKey("Reachable"))
        //    {
        //        Console.Out.WriteLine(dictionary.ToString());
        //        Console.Out.WriteLine("-------");
        //    }

        //    IOrderedEnumerable<string> sortedList =
        //        keyList.OrderBy(key => key.Length).ThenBy(key => key);

        //    foreach (string key in sortedList)
        //    {

        //    }
        //    return null;
        //}
    }
}


def tail(SentSequence):
    if SentSequence == [] or len(SentSequence) == 1:
        return []
    else:
        return SentSequence[1 : len(SentSequence)-1]
        
# checks the bot's previous sent message against the provided one, returning 1e if they match
def is_previous_message(bot, Msg):
    if bot.sent_msg_log == None or \
    len(bot.sent_msg_log) == 0 or \
    bot.sent_msg_log[-1] != Msg:
        return 0
        return 1
        
def send_if_not_prev(bot, Msg):
    if not is_previous_message(bot, Msg):
        bot.send_message(Msg[0], Msg[1])
        
def is_known_weapon_class(SentClass):
    if SentClass == None:
        return 0
    else:
        if SentClass.find("goowand") != -1:
            return 1
    return 0
