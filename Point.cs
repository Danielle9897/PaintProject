using System;


namespace ShapesGraphics
{
    [Serializable]
    // public class Point
    public struct Point 
    {
        // ----
        // Data
        // ----

        public int X { get; set; }
        public int Y { get; set; }

        // -------
        // Methods
        // -------

        public Point(int x, int y) : this()
        {
            this.X = x;
            this.Y = y;
        }

        // Return distance to (0,0) 
        public float Distance()
        {
            return Distance(X, 0, Y, 0);
        }
        // Return distance to (x,y)
        public float Distance(int x, int y)
        {
            return Distance(X, x, Y, x);
        }
        // Return distance to (p.x, p.y) 
        public float Distance(Point p)
        {
            return Distance(X, p.X, Y, p.Y);
        }       

        // Return distance between (x1,x2) & (y1,y2)
        private float Distance(int x1, int x2, int y1, int y2)
        {
            return Convert.ToSingle(Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2)));
        }

        public override string ToString()
        {
            return String.Format("({0},{1})", X.ToString(), Y.ToString());
        }

        //public override bool Equals(object obj)
        //{
        //    if (obj == null)
        //        return false;
        //    if (obj.GetType().Name != "Point")
        //        return false;
        //    Point p = (Point)obj;
        //    return (p.X == this.X) && (p.Y == this.Y);
        //}
    }
}

