using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CompVision
{
    public class Image
    {
        private int height;
        private int width;
        private double[,] pixels;
        //IEdgeEffect edgeEffect;
        public int getHeight()  {return height;}
        public void setHeight(int value) { height = value; }
        public int getWidth() {return width;}
        public void setWidth(int value) { width = value; }
        public double[,] getPixels() {return pixels;}

        public Image() { }
        public Image(int x, int y)
        {
            width = x;
            height = y;

            pixels = new double[width, height];
        }

        public Image(Image copy)
        {
            width = copy.width;
            height = copy.height;
            pixels = new double[width, height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixels[i, j] = copy.getPixels()[i, j];
                }
            }
        }

        public Image(Bitmap image) {
            width = image.Width;
            height = image.Height;

            pixels = new double[width, height];

            for (int i = 0; i<width; i++) {
                for (int j = 0; j<height; j++) {

                    Color pixel = image.GetPixel(i, j);
                    pixels[i, j] = 0.213 * pixel.R + 0.715 * pixel.G + 0.072 * pixel.B;
                }
            }
        }

        public double getPixel(int x, int y)
        {
            //Black Effect
            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                return 0;
            }
            return pixels[x,y];
        }

        public void setPixel(int x, int y, double pixel)
        {
            // Validation
            if (pixel < 0) pixel = 0;
            if (pixel > 255) pixel = 255;

            pixels[x,y] = (int)pixel;
        }

        public Bitmap getOutputImage() {
            Bitmap image = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image.SetPixel(i, j, Color.FromArgb((int)pixels[i, j], (int)pixels[i, j], (int)pixels[i, j]));

                }
            }
            return image;
        }
    }
}