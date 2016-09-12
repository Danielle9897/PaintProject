using System;
using System.Drawing;


namespace ShapesGraphics
{
    [Serializable]
    public class Rectangle : Shape
    {
        // ----
        // Data
        // ----

        private float width;
        private float height;

        // -------
        // Methods
        // -------

        public Rectangle(Point p, Color c, bool fill, Pen pen, Brush brush, float width, float height, Image image)
                  : base(p, c, fill, pen, brush, image)
        {
            // Throw exception if invalid width
            if (width <= 0)
            {
                ShapesException ex = new ShapesException("Rectangle Shape Constructor Exception",
                                                         "Error: Invalid width", DateTime.Now);
                ex.Data.Add("Width", width.ToString());
                throw ex;
            }
            else
                this.width  = width;

            // Throw exception if invalid height
            if (height <= 0)
            {
                ShapesException ex = new ShapesException("Rectangle Shape Constructor Exception",
                                                         "Error: Invalid height", DateTime.Now);
                ex.Data.Add("Height", height.ToString());
                throw ex;
            }
            else
                this.height = height;

            area = CalcArea();
        }

        public override void Draw(Graphics g) 
        {
            if (Show)
            {
                if (Fill)
                {                    
                    if (Image != null)
                    {
                        // Fill with image
                        g.DrawImage(Image, base.Position.X, base.Position.Y, width, height);
                    }
                    else
                        // Fill with chosen brush
                        g.FillRectangle(Brush, base.Position.X, base.Position.Y, width, height);
                }
                else
                    // Draw border only
                    g.DrawRectangle(Pen, base.Position.X, base.Position.Y, width, height);               
            }
        }

        public override void Resize(int percent) 
        {
            width  += (width  / 100) * percent;
            height += (height / 100) * percent;
            CalcArea();
        }

        public override double CalcArea() 
        {
            area = width * height;
            return area;
        }

        public override double CalcPerimeter()
        {
            return (width * 2) + (height * 2);
        }

        public override bool Contains(Point p)
        {
            if ((p.X >= Position.X) && (p.X <= Position.X + width) &&
                (p.Y >= Position.Y) && (p.Y <= Position.Y + height))
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return "Rectangle :: " + base.ToString() + String.Format(", Width: {0}, Height: {1}", width, height);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if(obj.GetType().Name != "Rectangle")
                return false;

            Rectangle r = (Rectangle)obj;
            return (r.width  == this.width)  &&
                   (r.height == this.height) &&
                    base.Equals(r); 
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
