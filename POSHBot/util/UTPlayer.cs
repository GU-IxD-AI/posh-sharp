using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp.POSHBot.util
{
    public class UTPlayer
    {
        public string Id { get; protected internal set; }
        public Vector3 Rotation { get; protected internal set; }
        public Vector3 Location { get; protected internal set; }
        public float Velocity { get; protected internal set; }
        public string Name { get; protected internal set; }
        public string Team { get; protected internal set; }
        public bool Reachable { get; protected internal set; }
        public string Weapon { get; protected internal set; }
        public int Firing { get; protected internal set; }

        private UTPlayer()
        {

        }

        public UTPlayer(Dictionary<string, string> attributes)
        {
            foreach (string key in attributes.Keys)
            {
                switch (key)
                {
                    case "Id":
                        Id = attributes["Id"].Trim();
                        break;
                    case "Rotation":
                        Rotation = Vector3.ConvertToVector3(attributes["Rotation"]);
                        break;
                    case "Location":
                        Location = Vector3.ConvertToVector3(attributes["Location"]);
                        break;
                    case "Velocity":
                        Velocity = POSHBot.CalculateVelocity((attributes["Velocity"]));
                        break;
                    case "Name":
                        Name = attributes["Name"].Trim();
                        break;
                    case "Team":
                        Team = attributes["Team"].Trim();
                        break;
                    case "Reachable":
                        Reachable = bool.Parse(attributes["Reachable"]);
                        break;
                    case "Weapon":
                        Weapon = attributes["Weapon"].Trim();
                        break;
                    case "Firing":
                        Firing = int.Parse((attributes["Firing"]));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
