using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    class Core
    {
        private int width;
        private int height;
        private double[,] core;
        public int getHeight() {return height;}
        public void setHeight(int value) { height = value; }
        public int getWidth() {return width;}
        public void setWidth(int value) { width = value; }
        public double getCoreAt(int x, int y) {return core[x,y];}

        public Core()
        {
        }
        
        public Core(int widthX, int heightX, double[,] coreX)
        {
            width = widthX;
            height = heightX;
            core = coreX;
        }
    }
}
