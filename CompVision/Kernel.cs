using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    class Kernel
    {   
        public int width { get; set; }
        public int height { get; set; }
        private double[,] kernel;
        public double getCoreAt(int x, int y) {return kernel[x,y];}

        public Kernel()
        {
        }
        
        public Kernel(int widthX, int heightX, double[,] coreX)
        {
            width = widthX;
            height = heightX;
            kernel = coreX;
        }
    }
}
