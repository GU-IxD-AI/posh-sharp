using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Posh_sharp.examples.BODBot.util;

namespace Posh_sharp.BODBot.util
{
    /// <summary>
    /// a UT INV object that can be picked up
    /// </summary>
    public class InvItem
    {
        public enum Event {See, Pickup };

        public string Id { get; protected internal set; }
        public Event UtEvent { get; protected internal set; }
        public int Amount { get; protected internal set; }
        public Vector3 Location { get; protected internal set; }
        public bool Reachable { get; protected internal set; }
        public string Class { get; protected internal set; }
        
        private InvItem()
        {

        }

        public InvItem(Dictionary<string, string> attributes)
        {
            foreach (string key in attributes.Keys)
            {
                switch (key)
                {
                    case "Id":
                        Id = attributes["Id"].Trim();
                        break;
                    case "Event":
                        // TODO: if events get more complicated create new method which handles the Event strings and returns the enum value 
                        UtEvent = (Event.Pickup.ToString().Equals(attributes["Event"])) ? Event.Pickup : Event.See;
                        break;
                    case "Amount":
                        Amount = int.Parse((attributes["Amount"]));
                        break;
                    case "Location":
                        Location = Vector3.ConvertToVector3(attributes["Location"]);
                        break;
                    case "Reachable":
                        Reachable = bool.Parse(attributes["Reachable"]);
                        break;
                    case "Class":
                        Class = (attributes["Class"]);
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// This method contains the infos on known weapons.
        /// </summary>
        /// <param name="sentClass"></param>
        /// <returns></returns>
        public bool IsKnownWeaponClass()
        {
            if (Class.Trim() == string.Empty)
                return false;
            
            if ( Class.Trim() == "goowand" )
                return true;
            
            return false;
        }
    }
}
