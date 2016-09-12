using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapesGraphics
{
    class SortLargeToSmall : IComparer<Shape>
    {
        // Sort List from large to small        
        int IComparer<Shape>.Compare(Shape x, Shape y)
        {            
            if (x.Area < y.Area) return 1;
            if (x.Area > y.Area) return -1;
            return 0;
        }
    }
}
