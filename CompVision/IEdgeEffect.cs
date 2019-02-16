using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    class IEdgeEffect
    {
        public virtual double getPixel(int x, int y, Image image) { return 0; }
    }
}
