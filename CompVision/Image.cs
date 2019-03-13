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
        public enum EdgeEffect { Black, Repeat, Mirror, Wrapping };
        private EdgeEffect edgeEffect;

        public EdgeEffect _EdgeEffect
        {
            get => edgeEffect;
            set => edgeEffect = value;
        }

        public int Height
        {
            get => height;
            set => height = value;
        }

        public int Width
        {
            get => width;
            set => width = value;
        }

        public Image()
        {
            width = 0;
            height = 0;
            edgeEffect = EdgeEffect.Repeat;
        }

        public Image(int x, int y, EdgeEffect xEdgeEffect)
        {
            width = x;
            height = y;
            pixels = new double[width * height];
            edgeEffect = xEdgeEffect;
        }

        public Image(Image copy)
        {
            width = copy.width;
            height = copy.height;
            pixels = new double[copy.width * copy.height];
            edgeEffect = copy.edgeEffect;
            //edgeEffect = EdgeEffect.Repeat;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    pixels[i + j * width] = copy.pixels[i + j * width];
                }
            }
        }

        public Image(Bitmap image, EdgeEffect xEdgeEffect)
        {
            width = image.Width;
            height = image.Height;
            edgeEffect = xEdgeEffect;

            pixels = new double[width * height];

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

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
            pixels[x + y * width] = pixel;
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
            int a = x;
            int b = y;

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

        public Bitmap getOutputImage()
        {
            Bitmap image = new Bitmap(width, height);
            Image outImage = getDeNormolize(this);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double pixel = outImage.getPixel(i, j);
                    image.SetPixel(i, j, Color.FromArgb(Convert.ToInt32(pixel),
                        Convert.ToInt32(pixel), Convert.ToInt32(pixel)));

                }
            }
            return image;
        }

        public static bool sizeEq(Image img1, Image img2)
        {
            return img1.width == img2.width && img1.height == img2.height;
        }

        public static Image operator -(Image left, Image right)
        {

            Image result = new Image(left.Width, left.Height, left.edgeEffect);

            for (int i = 0; i < left.Width; i++)
            {
                for (int j = 0; j < left.Height; j++)
                {
                    result.setPixel(i, j, left.getPixel(i, j) - right.getPixel(i, j));
                }
            }
            return result;
        }

        public double getMax()
        {
            return pixels.Max<double>();
        }

        public double getMin()
        {
            return pixels.Min<double>();
        }

        public static Image getDeNormolize(Image img)
        {
            Image outImage = new Image(img); //copy

            double min = img.getMin();
            double max = img.getMax();

            //если границы в пределах допустимого
            if (min >= 0 && max <= 1)
            {
                for (int i = 0; i < outImage.pixels.Count(); i++)
                {
                    outImage.pixels[i] *= 255;
                }
                return outImage;
            }

            double delta = max - min;
            if (delta == 0)
            {
                delta = max;
            }
            for (int i = 0; i < outImage.pixels.Count(); i++)
            {
                outImage.pixels[i] -= min;
                outImage.pixels[i] /= delta;
                outImage.pixels[i] *= 255;
            }
            return outImage;
        }

        public static Bitmap createFromIndex(Image image, int index, Pyramid pyramid)
        {
            Bitmap resultImage = new Bitmap(image.Width, image.Height);

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixel = pyramid.CoordinateTransform(i, j, index);
                    image.setPixel(i, j, pixel);
                }
            }
            resultImage = image.getOutputImage();
            return resultImage;
        }

        public static Bitmap createImageWithPoints(Image image, List<Point> points)
        {
            ImageConverter.normolize(image);
            Bitmap resultImage = image.getOutputImage();
            for (int i = 0; i < points.Count - 1; i++)
            {
                resultImage.SetPixel(points[i].x - 1, points[i].y - 1, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x - 1, points[i].y, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x - 1, points[i].y + 1, Color.FromArgb(255, 0, 0));

                resultImage.SetPixel(points[i].x, points[i].y - 1, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x, points[i].y, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x, points[i].y + 1, Color.FromArgb(255, 0, 0));

                resultImage.SetPixel(points[i].x + 1, points[i].y - 1, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x + 1, points[i].y, Color.FromArgb(255, 0, 0));
                resultImage.SetPixel(points[i].x + 1, points[i].y + 1, Color.FromArgb(255, 0, 0));
            }
            return resultImage;
        }

        public void normolizePixels()
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] /= 255;
            }
        }

        public static Bitmap glueImages(Bitmap imageLeft, Bitmap imageRight)
        {
            // max height
            var height = Math.Max(imageLeft.Height, imageRight.Height);

            Bitmap resultImage = new Bitmap(imageLeft.Width + imageRight.Width, height);

            // imageLeft
            for (var i = 0; i < imageLeft.Width; i++)
            {
                for (var j = 0; j < imageLeft.Height; j++)
                {
                    Color pixel = imageLeft.GetPixel(i, j);
                    resultImage.SetPixel(i, j, pixel);
                }
            }

            // imageRight
            for (var i = 0; i < imageRight.Width; i++)
            {
                for (var j = 0; j < imageRight.Height; j++)
                {
                    Color pixel = imageRight.GetPixel(i, j);
                    resultImage.SetPixel(i + imageLeft.Width, j, pixel);
                }
            }
            return resultImage;
        }

        public static void drawLines(Bitmap image, int firstWidth, List<Vector> similar)
        {
            Random rnd = new Random();
            Graphics line = Graphics.FromImage(image);
            Pen p;

            foreach (Vector vec in similar)
            {
                p = new Pen(Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256)), 1);
                Point p1 = vec.first.getInterPoint();
                Point p2 = vec.second.getInterPoint();
                line.DrawLine(p, p1.x, p1.y, p2.x + firstWidth, p2.y);
            }
        }
    }
}