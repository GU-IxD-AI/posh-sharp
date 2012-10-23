using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp_examples.BODBot.util
{
    public class UTPlayer
    {
        public string Id { public get; protected internal set; }
        public Vector3 Rotation { public get; protected internal set; }
        public Vector3 Location { public get; protected internal set; }
        public float Velocity { public get; protected internal set; }
        public string Name { public get; protected internal set; }
        public string Team { public get; protected internal set; }
        public bool Reachable { public get; protected internal set; }
        public string Weapon { public get; protected internal set; }
        public int Firing { public get; protected internal set; }

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
                        Velocity = float.Parse((attributes["Velocity"]));
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
