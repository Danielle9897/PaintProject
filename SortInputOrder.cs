using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapesGraphics
{
    public class SortInputOrder : IComparer<Shape>
    {
        // Sort list by input order field
        int IComparer<Shape>.Compare(Shape x, Shape y)
        {           
            if (x.shapeInputOrder > y.shapeInputOrder) return 1;
            if (x.shapeInputOrder < y.shapeInputOrder) return -1;
            return 0;
        }
    }
}
