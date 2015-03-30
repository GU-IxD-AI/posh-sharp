using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Posh_sharp.POSHBot.util
{
    public class NavPoint
    {
        public string   Id          { get; internal set; }
        public Vector3  Location    { get; private set; }
        public bool     Visible     { get; private set; }
        public bool     Reachable   { get; protected internal set; }
        public string   Item        { get; internal set; }
        public string   ItemClass   { get; internal set; }
        public Vector3  Rotation    { get; internal set; }
        public List<Neighbor> NGP   { get; internal set; }
        /// <summary>
        /// Type coressponds to the in-game attribute Flag
        /// </summary>
        public string Type { get; private set; }
        public int Owner { get; private set; }
        internal Dictionary<int,NavPoint> Paths { get; private set; }

        public static Dictionary<int, Vector3> ConvertToPath(Dictionary<string, string> rawNP)
        {
            // remove the ID key to leave just numbers
            rawNP.Remove("Id");
            Dictionary<int,Vector3> path = new Dictionary<int,Vector3>();
            // debug
            if (rawNP.ContainsKey("Reachable"))
            {
                //Console.Out.WriteLine(rawNP);
                //Console.Out.WriteLine("----------");
            }   

            // sorted string list regarding length and lexicographically problems might occur if a key pair would be 01 vs 2
            IOrderedEnumerable<string> sortedList = rawNP.Keys.OrderBy(key => key.Length).ThenBy(key => key);

            foreach (string key in sortedList)
            {
                path[int.Parse(key)] = Vector3.ConvertToVector3(rawNP[key].Split(new char[] {' '},1)[1]);
            }

            return path;
        }

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
                //Console.Out.WriteLine(dictRawNP.ToString());
                //Console.Out.WriteLine("-------");
            }

            IOrderedEnumerable<string> sortedList =
                keyList.OrderBy(key => key.Length).ThenBy(key => key);

            foreach (string key in sortedList)
            {
                string locString = dictRawNP[key];
                switch (key)
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
            this.NGP = new List<Neighbor>();
        }

		public NavPoint(string id, int owner, Vector3 location, Dictionary<int,NavPoint> paths) : this(id, owner, location, paths, "")
		{}

        public NavPoint(string id, int owner, Vector3 location, Dictionary<int,NavPoint> paths, string type) : this()
        {
            this.Id = id;
            this.Type = type;
            this.Owner = owner;
            this.Location = location;
            this.Paths = paths;
        }

        public float Distance2DFrom(NavPoint target, Vector3.Orientation orientation)
        {
            return Location.Distance2DFrom(target.Location, orientation);
        }

        public float Distance2DFrom(Vector3 target, Vector3.Orientation orientation)
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

        public void SetNeighbors()
        {
            this.NGP = new List<Neighbor>();
        }

        public class Neighbor
        {
            public readonly string Id;
            public readonly int Flags;
            public readonly int CollisionR;
            public readonly int CollisionH;
            public readonly bool ForceDoubleJump;
            public readonly bool OnlyTranslocator;
            
            public Neighbor(Dictionary<string,string> ngp)
            {
                foreach(KeyValuePair<string,string> elem in ngp)
                    switch (elem.Key)
                    {
                        case "Id":
                            Id = elem.Value;
                            break;
                        case "Flags":
                            Flags = int.Parse(elem.Value);
                            break;
                        case "CollisionR":
                            CollisionR = int.Parse(elem.Value);
                            break;
                        case "ForceDoubleJump":
                            ForceDoubleJump = bool.Parse(elem.Value);
                            break;
                        case "OnlyTranslocator":
                            OnlyTranslocator = bool.Parse(elem.Value);
                            break;
                    }
                
            }
        }
    }
}
