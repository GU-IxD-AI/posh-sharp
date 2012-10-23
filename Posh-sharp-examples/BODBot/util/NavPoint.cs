using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp_examples.BODBot.util
{
    public class NavPoint
    {
        public string   Id          { public get; internal set; }
        public Vector3  Location    { public get; private set; }
        public bool     Visible     { public get; private set; }
        public bool     Reachable   { public get; protected internal set; }
        public string   Item        { public get; internal set; }
        public string   ItemClass   { public get; internal set; }
        public Vector3  Rotation    { public get; internal set; }
        /// <summary>
        /// Type coressponds to the in-game attribute Flag
        /// </summary>
        public string Type { public get; private set; }
        public int Owner { public get; private set; }
        public Dictionary<int,NavPoint> paths { internal get; private set; }
        


        /// <summary>
        /// lists of nav points arrive as dicts with an "ID" key and keys "0", "1", .... "n" these need converting to lists
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        public static NavPoint ConvertToNavPoint(Dictionary<string, string> dictRawNP)
        {
            NavPoint location = new NavPoint();

            // now get a list of just keys, and sort it to use in extracting the key:value pairs
            Dictionary<string, string>.KeyCollection keyList = dictRawNP.Keys;

            // debug
            if (dictRawNP.ContainsKey("Reachable"))
            {
                Console.Out.WriteLine(dictRawNP.ToString());
                Console.Out.WriteLine("-------");
            }

            IOrderedEnumerable<string> sortedList =
                keyList.OrderBy(key => key.Length).ThenBy(key => key);

            foreach (string key in sortedList)
            {
                string locString = dictRawNP[key];
                switch (locString)
                {
                    case "Id":
                        location.Id = locString.Trim();
                        break;
                    case "Location":
                        location.Location = Vector3.ConvertToVector3(locString);
                        break;
                    case "Visible":
                        location.Visible = bool.Parse(locString.Trim());
                        break;
                    case "Reachable":
                        location.Reachable = bool.Parse(locString.Trim());
                        break;
                    case "Item":
                        location.Item = locString.Trim();
                        break;
                    case "ItemClass":
                        location.ItemClass = locString.Trim();
                        break;
                    case "Flag":
                        location.Type = locString.Trim();
                        break;
                    case "Rotation":
                        location.Rotation = Vector3.ConvertToVector3(locString);
                        break;
                    default:
                        break;


                }
            }

            return location;
        }

        private NavPoint()
        {
        }

        public NavPoint(string id, int owner, Vector3 location, Dictionary<int,NavPoint> paths, string type = "")
        {
            this.Id = id;
            this.Type = type;
            this.Owner = owner;
            this.Location = location;
            this.paths = paths;
        }

        public float Distance2DFrom(NavPoint target, Vector3.Orientation orientation = Vector3.Orientation.XY)
        {
            return Location.Distance2DFrom(target.Location, orientation);
        }

        public float Distance2DFrom(Vector3 target, Vector3.Orientation orientation = Vector3.Orientation.XY)
        {
            return Location.Distance2DFrom(target, orientation);
        }

        public float DistanceFrom(NavPoint target)
        {
            return Location.DistanceFrom(target.Location);
        }

        public float DistanceFrom(Vector3 target)
        {
            return Location.DistanceFrom(target);
        }

        
    }
}
