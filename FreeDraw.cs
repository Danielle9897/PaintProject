using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapesGraphics
{
    [Serializable]
    class FreeDraw : Shape
    {
        // ----
        // Data
        // ----        

        private List<Point> freeDrawShape;

        // -------
        // Methods
        // -------
        public FreeDraw(Point startPoint, Color color,  List<Point>pointsList, Pen pen, Brush brush)
                          : base(startPoint, color, true, pen, brush)          
        {
            this.freeDrawShape = pointsList;
            area = CalcArea();            
        }
        public override object Clone()
        {
            FreeDraw cloneShape = (FreeDraw)base.Clone();
            cloneShape.freeDrawShape = new List<Point>(this.freeDrawShape);
            return cloneShape;
        }
        public override void MoveLocation(Point newStartPosition)
        {
            // Calc new x,y data
            int deltaX = newStartPosition.X - freeDrawShape[0].X;
            int deltaY = newStartPosition.Y - freeDrawShape[0].Y;

            for (int i = 0; i < freeDrawShape.Count; i++)
            {                
                freeDrawShape[i] = new Point(freeDrawShape[i].X + deltaX, freeDrawShape[i].Y + deltaY);
            }
        }
        public override double CalcArea()
        {
            // Consider area as: line length * pen width
            area = CalcPerimeter() * Pen.Width;
            return area;
        }
        public override double CalcPerimeter()
        {
            // NOTE: Consider perimeter as the length....                    

            double length = 0;
            for (int i = 0; i < freeDrawShape.Count-2; i++)
            {
                length += freeDrawShape[i].Distance(freeDrawShape[i + 1]);
            }

            return length;
        }
        public override void Draw(Graphics g)
        {
            if (Show)
            {
                for (int i = 0; i < freeDrawShape.Count-1; i++)
                     g.DrawLine(Pen, freeDrawShape[i].X, freeDrawShape[i].Y, freeDrawShape[i+1].X, freeDrawShape[i+1].Y);
            }
        }
        public override void Resize(int percent)
        {
            // Resize the line width !
            float temp = Pen.Width + ((Pen.Width / 100) * percent);
            Pen.Width = (temp < 1) ? 1 : temp;
            CalcArea();
        }
        public override bool Contains(Point p)
        {
            bool found = false;
            int range = (int)(Pen.Width / 2);
            if (Pen.Width < 3) range = 2;

            for (int i = 0; i < freeDrawShape.Count; i++)
            {
                // 1. Check exact hit; Add range since its difficult to right click exactly on a pixel    
                if ((freeDrawShape[i].X >= p.X - range) &&
                    (freeDrawShape[i].X <= p.X + range) &&
                    (freeDrawShape[i].Y >= p.Y - range) &&
                    (freeDrawShape[i].Y <= p.Y + range))
                {
                    found = true;
                    break;
                }

                // 2. Check if p is anywhere between the points in the array 
                //    (When we draw fast there are very few points in the array)
                if (i < freeDrawShape.Count-1)
                {
                    float a = freeDrawShape[i].Distance(p);
                    float b = freeDrawShape[i+1].Distance(p);
                    if ((int)(a + b) == (int)(freeDrawShape[i].Distance(freeDrawShape[i+1])))
                    {
                        found = true;
                        break;
                    }
                }
            }
           
            return found;
        }
        public override string ToString()
        {
            //return "Free Draw :: Start " + base.ToString() + String.Format(", Length {0}", freeDrawShape.Count);
            return "Free Draw :: Start " + base.ToString() + String.Format(", Length {0}", Convert.ToInt32(CalcPerimeter()).ToString());
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj.GetType().Name != "FreeDraw")
                return false;

            FreeDraw fd = (FreeDraw)obj;

            if (this.freeDrawShape.Count != fd.freeDrawShape.Count)
                return false;

            for (int i = 0; i < this.freeDrawShape.Count; i++)
                if ((this.freeDrawShape[i].X != fd.freeDrawShape[i].X) || (this.freeDrawShape[i].Y != fd.freeDrawShape[i].Y))
                    return false;

            return base.Equals(fd);
        }
        public override int GetHashCode()
        {
            long num = 0;
            for (int i = 0; i < this.freeDrawShape.Count; i++)
                 num = num + this.freeDrawShape[i].X + this.freeDrawShape[i].Y;
            return num.GetHashCode(); 
        }
    }
}
