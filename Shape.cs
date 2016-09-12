using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;


namespace ShapesGraphics
{
    [Serializable]
    public abstract class Shape : IComparable<Shape>, ICloneable
    {
        // ----
        // Data
        // ----    

        public Point Position { get; set; } // Shape start position 
        public Color Color    { get; set; } 
        public bool  Fill     { get; set; }
        public Image Image    { get; set; }

        [NonSerialized]  // Will be serialized separately below with [OnSerializing]...
        private Pen pen;
        public Pen Pen { get { return pen; } set { pen = value; } }        
        
        [NonSerialized]  // Will be serialized separately below with [OnDeSerialized]...
        private Brush brush;      
        public Brush Brush { get { return brush; } set { brush = value; } }              

        protected double area;
        public double Area    { get { return area; } }

        private static int inputOrder = 0; // Static counter, used by the Icomparer sort !!!
                                           // Increased by one for each shape added.
        public static int InputOrder
        {
            get { return inputOrder;  }
            set { inputOrder = value; }
        }

        public int shapeInputOrder;       // The shape-object input order number that it has recieved when it was added
                                          // Used in SortInputOrder.cs
        public bool Show { get; set; }    // "To show or not to show the shape.."
        
        // ----
        // Data for Serialization -> Needed because Pen & Brush objects are NOT marked [Serializable] by the .Net 
        // So all the below is serializabe but Pen and Brush themselves are not...
        // (Other option, which I didn't use, was using ISerializationSurrogate)
        // ----        
        private float penWidth;
        private int   penDashStyle;
        private Type  brushType; 
        private Color brushColor1, brushColor2;
        private int   brushHatchStyle;        
        private RectangleF brushRectangle;        

        // -------
        // Methods
        // -------
                
        public abstract void   Draw(Graphics g);
        public abstract void   Resize(int percent);
        public abstract double CalcArea();
        public abstract double CalcPerimeter();
        public abstract bool   Contains(Point p);
        
        // Constructors
        public Shape(Point p, Color c, bool fill, Pen pen, Brush brush, Image image) 
                      : this (p, c, fill, pen, brush)
        {
            this.Image = image; 
        }

        public Shape(Point p, Color c, bool fill, Pen pen, Brush brush)
        {
            this.Color = c;
            this.Position = p;
            this.Fill = fill;
            this.Pen = pen;
            this.Brush = brush;           

            this.Show = true;
            shapeInputOrder = inputOrder++;
        }


        public virtual void MoveLocation(Point newPosition)
        {
            this.Position = newPosition;            
        }     

        public override string ToString()
        {
            return String .Format("Position: {0}, {1}, Fill: {2}", Position.ToString(), Color, Fill);
        }

        public override bool Equals(object obj)
        {  
            if (obj == null)
                return false;
            if ((obj as Shape) == null)
                return false;
            Shape p = (Shape)obj;

            return this.Position.Equals(p.Position) &&
                   this.Color.Equals(p.Color)       &&
                   (this.Fill == p.Fill);
        }
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        int IComparable<Shape>.CompareTo(Shape other)
        {
            // This CompareTo method compares areas - Small to Large      
            // Note: I made 2 more IComparer classes to sort by 'Large to Small' and to sort by 'input order' !
            if (this.area  > other.area) return  1;
            if (this.area  < other.area) return -1;
            return 0; 
        }
        public virtual object Clone()
        {
            // 1. Get a shallow copy
            Shape cloneShape = (Shape)this.MemberwiseClone();

            // 2. Add deep copy  
            cloneShape.Pen   = (Pen)this.Pen.Clone();
            cloneShape.Brush = (Brush)this.Brush.Clone();

            return cloneShape;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
             // Save pen and brush data implicitly .....since they are NOT serializabe by .net             
             penWidth        = Pen.Width;
             penDashStyle    = (int)Pen.DashStyle; // to be checked later for default value            
             brushType       = Brush.GetType();

             if (brushType == typeof(LinearGradientBrush))
             {
                 brushColor1 = ((LinearGradientBrush)Brush).LinearColors[0];
                 brushColor2 = ((LinearGradientBrush)Brush).LinearColors[1];                 
                 brushRectangle = ((LinearGradientBrush)(Brush)).Rectangle;
             }
             if (brushType == typeof(HatchBrush))
             {
                 brushColor1     = ((HatchBrush)Brush).BackgroundColor;
                 brushColor2     = ((HatchBrush)Brush).ForegroundColor;
                 brushHatchStyle = (int)((HatchBrush)Brush).HatchStyle;
             }             
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // *** Restore Pen
            Pen = new Pen(Color, penWidth);
            Pen.DashStyle = (DashStyle)penDashStyle;

            // *** Restore Brush
            Brush = new SolidBrush(Color);
            Type type = brushType;
            
            if (brushType == typeof(LinearGradientBrush))
            {
                // Get a random number for the Mode
                Array arr = Enum.GetValues(typeof(LinearGradientMode));
                LinearGradientMode lgm = (LinearGradientMode)arr.GetValue(new Random().Next(arr.Length));
                brush = new LinearGradientBrush(brushRectangle, brushColor1, brushColor2, lgm);       
            }
            if (type == typeof(HatchBrush))
            {
                brush = new HatchBrush((HatchStyle)brushHatchStyle, brushColor2, brushColor1);
            }
        }


    }
}
