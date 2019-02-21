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
        private double[] kernel;
        public double getkernelAt(int x, int y) {return kernel[x + y * width]; }

        public Kernel()
        {
        }
        
        public Kernel(int widthX, int heightX, double[] kernelX)
        {
            width = widthX;
            height = heightX;
            kernel = kernelX;
        }

        public void rotate()
        {
            int i = width;
            width = height;
            height = i;
        }

    }
}
