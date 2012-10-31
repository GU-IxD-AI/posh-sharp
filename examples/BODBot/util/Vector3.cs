using System;

namespace Posh_sharp.examples.BODBot.util
{
    class Vector3
    {
        public float X { public get; private set; }
        public float Y { public get; private set; }
        public float Z { public get; private set; }

        public enum Orientation {XY,XZ,YZ};

        /// <summary>
        /// takes a string of the form 'x,y,z' and converts it to a tuple (x,y,z)
        /// </summary>
        /// <param name="location">a string of three floats separated by ','</param>
        /// <returns>returns a tuple (x,y,z)</returns>
        public static Vector3 ConvertToVector3(string location)
        {
            string[] locList = location.Split(',');
            if (locList.Length != 3)
                return new Vector3();

            return new Vector3(float.Parse(locList[0]), float.Parse(locList[1]), float.Parse(locList[2]));
        }

        public Vector3(float x = 0, float y = 0, float z = 0)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float DistanceFrom(Vector3 origin = null)
        {
            if (origin == null)
                return (float)Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
            else
                return (float)Math.Sqrt(Math.Pow(X-origin.X, 2) + Math.Pow(Y-origin.Y, 2) + Math.Pow(Z-origin.Z, 2));
        }

        public float Distance2DFrom(Vector3 vector,Orientation orientation = Orientation.XY)
        {
            switch (orientation)
            {
                case Orientation.XZ:
                    return (float)Math.Sqrt(Math.Pow(X - vector.X, 2) + Math.Pow(Z - vector.Z, 2));
                case Orientation.YZ:
                    return (float)Math.Sqrt(Math.Pow(Y - vector.Y, 2) + Math.Pow(Z - vector.Z, 2));
                default: //XY
                    return (float)Math.Sqrt(Math.Pow(X - vector.X, 2) + Math.Pow(Y - vector.Y, 2));
            }
        }

        public Vector3 Add(Vector3 vector)
        {
            return new Vector3(X+vector.X,Y+vector.Y,Z+vector.Z);
        }

        public Vector3 Subtract(Vector3 vector)
        {
            return new Vector3(X - vector.X, Y - vector.Y, Z - vector.Z);
        }

        public Vector3 Mult(int multiplier)
        {
            return new Vector3(X*multiplier, Y*multiplier, Z*multiplier);
        }

        public float Norm()
        {
            return DistanceFrom();
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", X, Y, Z);
        }
    }

}
