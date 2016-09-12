using System;
using System.Collections.Generic;
using System.Drawing;

namespace ShapesGraphics
{    
    [Serializable]
    class ShapeContainer
    {  
        private List<Shape> shapesList;
    
        // Events for min/max area size handling
        // Note: Can't be serialized;
        //       Form needs to resubscribe after deserialization !
        [field:NonSerialized]
        public event Action<bool> tooSmallAreaEvent;
        [field:NonSerialized]
        public event Action<bool> tooLargeAreaEvent;

        public const int MIN_AREA = 20;
        public const int MAX_AREA = 1500000;

        private double largestArea;
        private double smallestArea;
       
        public ShapeContainer(int size)
        {            
            shapesList   = new List<Shape>(size);

            largestArea  = 0;
            smallestArea = MAX_AREA; 
        }

        // Indexer
        public Shape this[int index]
        {
            get
            {
                if (index < 0 || index >= shapesList.Count)
                {
                    throw new IndexOutOfRangeException("MyMessage: Index out of range: " + index.ToString() +
                                                       "; Count is: " + shapesList.Count);
                }
                else
                    return shapesList[index];
            }
            set
            {
                if (index >= 0 && index < shapesList.Count)                   
                    shapesList[index] = value;
                else 
                {
                    throw new IndexOutOfRangeException("MyMessage: Index out of range: " + index.ToString() +
                                                       "; Count is: " + shapesList.Count);
                } 
            }
        }

        // Add an element
        public void Add(Shape s)
        {           
            shapesList.Add(s);        
        }         

        // Remove an element by index
        public void Remove(int index)
        {  
            if ((shapesList.Count >= 1) && (index >= 0))
                 shapesList.RemoveAt(index);     
        }

        // Remove element
        public bool Remove(Shape s)
        {           
            return shapesList.Remove(s); 
        }

        // Remove all / Clear all
        public void RemoveAll()
        {
            shapesList.Clear();
        }

        public void RemoveLastInput()
        {
            int highOrder = 0;
            int highIndex = 0;

            for (int i = 0; i < shapesList.Count; i++ )
                {
                    // find the shape with the highest input order...
                    if (shapesList[i].shapeInputOrder > highOrder)
                    {
                        highOrder = shapesList[i].shapeInputOrder;
                        highIndex = i;
                    }
                }
            
            Remove(highIndex);
        }

        // Resize all 
        public void ResizeAll(int percent)
        {           
            for (int i = 0 ; i < shapesList.Count; i++)
            {                
                shapesList[i].Resize(percent);
            }
        }

        // Draw all
        public void DrawAll(Graphics g)
        {
            largestArea  = 0;
            smallestArea = MAX_AREA;
                       
            for (int i = 0; i < shapesList.Count; i++)
            {  
                // 1. Draw all
                shapesList[i].Draw(g);

                // 2. Update area info
                if (shapesList[i].Area > largestArea)
                    largestArea = shapesList[i].Area;
                if (shapesList[i].Area < smallestArea)
                    smallestArea = shapesList[i].Area;    
            }

            // 3. Invoke event methods if needed, 
            //    if area is too small or too large
            if (tooSmallAreaEvent != null)
                if (smallestArea <= MIN_AREA)
                    tooSmallAreaEvent(true);
                else
                    tooSmallAreaEvent(false);

            if (tooLargeAreaEvent != null)
                if (largestArea >= MAX_AREA)               
                    tooLargeAreaEvent(true);
                else
                    tooLargeAreaEvent(false);
        }

        // Get count
        public int Size()
        {
            return shapesList.Count;
        }

        // Sort Shapes by IComarer object
        public void Sort(IComparer<Shape> sortObj)
        {            
             shapesList.Sort(sortObj);
        }
        
        // Sort Shapes by size (area)
         public void Sort()
        {            
             shapesList.Sort();          
        }

        public void SetShowType(ShapeType type)
        {           
            for (int i = 0; i < shapesList.Count; i++)
            {
                switch (shapesList[i].GetType().Name)
                {
                    case "Line":
                        {
                            if ((type & ShapeType.LINE) == ShapeType.LINE)
                                shapesList[i].Show = true;
                            else
                                shapesList[i].Show = false;
                            break;
                        }
                    case "Circle":
                        {
                            if ((type & ShapeType.CIRCLE) == ShapeType.CIRCLE)
                                shapesList[i].Show = true;
                            else
                                shapesList[i].Show = false;
                            break;
                        }
                    case "Rectangle":
                        {
                            if ((type & ShapeType.RECTANGLE) == ShapeType.RECTANGLE)
                                shapesList[i].Show = true;
                            else
                                shapesList[i].Show = false;
                            break;
                        }
                    case "FreeDraw":
                        {
                            if ((type & ShapeType.FREE) == ShapeType.FREE)
                                shapesList[i].Show = true;
                            else
                                shapesList[i].Show = false;
                            break;
                        }
                }         
            }
        }

        public Shape GetShapeAtPosition(Point p)
        {
            int? index = null;            

            // Loop on list and find the first shape that contains point p
            for (int i = shapesList.Count - 1; i >= 0; i--)
            {
                if ((shapesList[i].Contains(p)) && (shapesList[i].Show))
                {
                    index = i;
                    break;
                }
            }

            if (index.HasValue)           
                return shapesList[(int)index];
            else
                return null;
        }

        public void ChangeShapeInputOrder(Shape shape, int position)
        {
            // *** Send to back *** (position 0 is drawn first)
            if (position == 0)
            { 
                // Move all shapes (only those that are in a smaller position) 1 number up
                for (int i = 0; i < shapesList.Count; i++)
                {
                    if (shapesList[i].shapeInputOrder < shape.shapeInputOrder)
                        shapesList[i].shapeInputOrder++;
                }
                // Set this shape as the first one
                shape.shapeInputOrder = 0;
            }

            // *** Bring to front *** (will be drawn last)
            else if (position == 1)
            {
                // Move all shapes (only those that are in a larger position) 1 number down
                for (int i = 0; i < shapesList.Count; i++)
                {
                    if (shapesList[i].shapeInputOrder > shape.shapeInputOrder)
                        shapesList[i].shapeInputOrder--;
                }
                // Set this shape as the last one
                shape.shapeInputOrder = Shape.InputOrder-1;
            }
        }
    }
}
