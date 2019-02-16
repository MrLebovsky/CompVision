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
        private double[] pixels;
        public int getHeight()  {return height;}
        public void setHeight(int value) { height = value; }
        public int getWidth() {return width;}
        public void setWidth(int value) { width = value; }
        public double[] getPixels() {return pixels;}
        public enum EdgeEffect { Black, Repeat, Mirror, Wrapping };
        private EdgeEffect edgeEffect;

        public Image() { }

        public Image(int x, int y)
        {
            width = x;
            height = y;
            pixels = new double[width * height];
            edgeEffect = EdgeEffect.Black;
        }

        public Image(Image copy)
        {
            width = copy.width;
            height = copy.height;
            pixels = new double[copy.width * copy.height];
            edgeEffect = copy.edgeEffect;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixels[i + j * width] = copy.pixels[i + j * width];
                }
            }
        }

        public Image(Bitmap image, EdgeEffect xEdgeEffect) {
            width = image.Width;
            height = image.Height;
            edgeEffect = xEdgeEffect;

            pixels = new double[width * height];

            for (int i = 0; i<width; i++) {
                for (int j = 0; j<height; j++) {

                    Color pixel = image.GetPixel(i, j);
                    pixels[i + j * width] = 0.213 * pixel.R + 0.715 * pixel.G + 0.072 * pixel.B;
                }
            }
        }

        public double getPixel(int x, int y)
        {
            if (x < width && x > -1 && y < height && y > -1)
                return pixels[x + y * width];

            switch (edgeEffect)
            {
                case EdgeEffect.Black:
                    return 0;
                case EdgeEffect.Repeat:
                    return getPixelRepeat(x, y);
                case EdgeEffect.Mirror:
                    return getPixelMirror(x, y);
                case EdgeEffect.Wrapping:
                    return getPixelWrapping(x, y);
            }
            return 0;
        }

        public void setPixel(int x, int y, double pixel)
        {
            // Validation
            if (pixel < 0) pixel = 0;
            if (pixel > 255) pixel = 255;

            pixels[x + y * width] = (int)pixel;
        }

        public double getPixelRepeat(int x, int y)
        {
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x >= width) x = width - 1;
            if (y >= height) y = height - 1;

            return pixels[x + y * width];
        }

        public double getPixelMirror(int x, int y)
        {
            if (x < 0) x = -x;
            if (y < 0) y = -y;
            if (x >= width) x = 2 * width - x - 1;
            if (y >= height) y = 2 * height - y - 1;

            return pixels[x + y * width];
        }

        public double getPixelWrapping(int x, int y)
        {
            if (x < 0) x = width + x;
            if (y < 0) y = height + y;
            if (x >= width) x = 1 + (x - width);
            if (y >= height) y = 1 + (y - height);

            return pixels[x + y * width];
        }

        public void setEdgeEffect(EdgeEffect xEdgeEffect)
        {
            edgeEffect = xEdgeEffect;
        }

        public Bitmap getOutputImage() {
            Bitmap image = new Bitmap(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double pixel = pixels[i + j * width];
                    image.SetPixel(i, j, Color.FromArgb((int)pixel, (int)pixel, (int)pixel));

                }
            }
            return image;
        }
    }
}