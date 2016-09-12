using System;
using System.Drawing;


namespace ShapesGraphics
{
    [Serializable]
    public class Line : Shape
    {
        // ----
        // Data
        // ----
              
        private Point endPoint;

        // -------
        // Methods
        // -------

        public Line(Point startPoint, Color color, Point endPoint, Pen pen, Brush brush) : base(startPoint, color, true, pen, brush)          
        {
            this.endPoint = endPoint;
            area = CalcArea();            
        }

        public override void Draw(Graphics g)
        {        
            if (Show)     
                g.DrawLine(Pen, base.Position.X, base.Position.Y, endPoint.X, endPoint.Y);                        
        }

        public override void MoveLocation(Point newStartPosition) 
        {         
            // Calc new End Point
            int newEndX = endPoint.X + (newStartPosition.X - Position.X);
            int newEndY = endPoint.Y + (newStartPosition.Y - Position.Y);
            endPoint = new Point(newEndX, newEndY);

            // Set the new Start Point
            base.MoveLocation(newStartPosition);
        }

        public override void Resize(int percent)
        {           
            // Resize line length... Change EndPoint values...
            int distanceX = endPoint.X - Position.X;
            int distanceY = endPoint.Y - Position.Y;            
            Point p = new Point(( Convert.ToInt32((float)distanceX / 100 * percent) + endPoint.X), 
                                ( Convert.ToInt32((float)distanceY / 100 * percent) + endPoint.Y));

            endPoint = p; // copy by value, since it is struct..

            //EndPoint.X += (EndPoint.X / 100 * percent);
            //EndPoint.Y += (EndPoint.Y / 100 * percent);            

            CalcArea();
        }

        public override double CalcArea()
        {
            // Consider area as: line length 
            area = CalcPerimeter();
            return area;
        }

        public override double CalcPerimeter()
        {
            // NOTE: 
            // Perimeter of a line is actually its length....
            // As a line is actually a rectangle with height == 0 .... 

            return  base.Position.Distance(endPoint);
        }

        public override bool Contains(Point p)
        {
            // Find the distance of point P from both line end points A, B. 
            // If AB = AP + PB, then P lies on the line segment AB.
            float a = Position.Distance(p);
            float b = endPoint.Distance(p);
            if ((int)(a + b) == (int)(area))
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "Line :: " + base.ToString() + String.Format(", End Point: {0}", endPoint);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType().Name != "Line")
                return false;

            Line l = (Line)obj;
            return (Point.Equals(this.endPoint, l.endPoint)) &&
                    base.Equals(l); 
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
