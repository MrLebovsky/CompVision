using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    class BlackEdgeEffect
    {
        public BlackEdgeEffect() { }

        public virtual double getPixel(int x, int y, Image image)
        {
            if (x < 0 || y < 0 || x >= image.getWidth() || y >= image.getHeight())
            {
                return 0;
            }
            return image.getPixels()[x, y];
        }
    }
}
