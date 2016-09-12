using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;


namespace ShapesGraphics
{
    [Serializable]
    public class Circle : Shape
    {
        // ----
        // Data
        // ----
                
        private float radius;

        // -------
        // Methods
        // -------

        public Circle(Point p, Color c, bool fill, Pen pen, Brush brush, float radius, Image image) 
               : base(p, c, fill, pen, brush, image)
        {
            // Throw exception if invalid radius
            if (radius <= 0)
            {
                ShapesException ex = new ShapesException("Circle Shape Constructor Exception",
                                                         "Error: Invalid radius", DateTime.Now);
                ex.Data.Add("Radius", radius.ToString());
                throw ex;
            }
            else
            {
                this.radius = radius;
                area = CalcArea();
            }
        }

        public override void Draw(Graphics g)
        {
            if (Show)
            {
                if (Fill)                                   
                    g.FillEllipse(Brush, base.Position.X, base.Position.Y, radius, radius);                
                else
                    g.DrawEllipse(Pen, base.Position.X, base.Position.Y, radius, radius);               
            }
        }

        public override void Resize(int percent)
        {  
            radius = radius + ((radius / 100) * percent);
            CalcArea();
        }

        public override double CalcArea()
        {
            area = Math.PI * radius * radius;
            return area;
        }

        public override double CalcPerimeter()
        {
            return (2 * Math.PI * radius);
        }

        public override bool Contains(Point p)
        {
            if ((p.X >= Position.X) && (p.X <= Position.X + radius) &&
                (p.Y >= Position.Y) && (p.Y <= Position.Y + radius))
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "Circle :: " + base.ToString() + String.Format(", Radius: {0}", radius);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType().Name != "Circle")
                return false;

            Circle c = (Circle)obj;
            return (c.radius == this.radius) && base.Equals(c); 
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
