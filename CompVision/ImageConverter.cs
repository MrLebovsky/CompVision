using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace CompVision
{
    class ImageConverter
    {
        ImageConverter() { }

        public static Image convolution(Image image, Kernel kernel)
        {
            Image resultImage = new Image(image.Width, image.Height, image._EdgeEffect);

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double resultPixel = 0;
                    for (int x = 0; x < kernel.width; x++)
                    {
                        for (int y = 0; y < kernel.height; y++)
                        {
                            int realI = i + (x - (kernel.width / 2));
                            int realJ = j + (y - (kernel.height / 2));

                            //тут
                            /*if ((realI < 0) ||
                                 (realJ >= image.Width) ||
                                 (realI < 0) ||
                                 (realJ >= image.Height)) continue;*/

                            resultPixel += image.getPixel(realI, realJ) * kernel.getkernelAt(x, y);
                        }
                    }
                    resultImage.setPixel(i, j, resultPixel);
                }
            }

            return resultImage;
        }

        public static void sobel(Image image)
        {
            Image copyImageX = convolution(image, KernelCreator.getSobelX());
            Image copyImageY = convolution(image, KernelCreator.getSobelY());

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixelX = copyImageX.getPixel(i, j);
                    double pixelY = copyImageY.getPixel(i, j);
                    image.setPixel(i, j, Math.Sqrt(pixelX * pixelX + pixelY * pixelY));
                }
            }
        }

        public static void priut(Image image)
        {
            Image copyImageX = convolution(image, KernelCreator.getPriutX());
            Image copyImageY = convolution(image, KernelCreator.getPriutY());

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixelX = copyImageX.getPixel(i, j);
                    double pixelY = copyImageY.getPixel(i, j);
                    image.setPixel(i, j, Math.Sqrt(pixelX * pixelX + pixelY * pixelY));
                }
            }
        }

        public static void normolize(Image image)
        {
            //Normolize
            double max = image.getMax();
            double min = image.getMin();

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixel = (image.getPixel(i, j) - min) * (255 / (max - min));
                    image.setPixel(i, j, pixel);
                }
            }
        }

        public static Image halfReduce(Image image)
        {
            Image resultImage = new Image(image.Width / 2, image.Height / 2, Image.EdgeEffect.Repeat);

            for (int i = 0; i < resultImage.Width; i++)
            {
                for (int j = 0; j < resultImage.Height; j++)
                {
                    double resullPixel = (image.getPixel(2 * i, 2 * j) + image.getPixel(2 * i + 1, 2 * j) +
                                  image.getPixel(2 * i, 2 * j + 1) + image.getPixel(2 * i + 1, 2 * j + 1)) / 4;
                    resultImage.setPixel(i, j, resullPixel);
                }
            }
            return resultImage;
        }

        public static Image noise(Image image, int count)
        {
            Random rand = new Random();
            double noise = image.getMax() - image.getMin();
            Image resultImage = new Image(image);
            for (int i = 0; i < count; i++)
            {
                resultImage.setPixel(rand.Next(1, image.Width), rand.Next(1, image.Height), noise);
            }
            return resultImage;
        }

        public static Image rotate(Image image)
        {
            Image resultImage = new Image(image.Height, image.Width, image._EdgeEffect);
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    resultImage.setPixel(image.Height - 1 - j, i, image.getPixel(i, j));
                }
            }
            return resultImage;
        }

        public static Image Brightness(Image image, int poz, int lenght)
        {
            int N = (100 / lenght) * poz; //кол-во процентов
            Image resultImage = new Image(image.Width, image.Height, image._EdgeEffect);

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixel = image.getPixel(i, j);
                    pixel = pixel + N * 128 / 100;

                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;

                    resultImage.setPixel(i, j, pixel);
                }
            }
            return resultImage;
        }

        public static Image Сontrast(Image image, int poz, int lenght)
        {
            int N = (100 / lenght) * poz; //кол-во процентов
            Image resultImage = new Image(image.Width, image.Height, image._EdgeEffect);

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixel = image.getPixel(i, j);
                    if (N >= 0)
                    {
                        if (N == 100) N = 99;
                        pixel = (pixel * 100 - 128 * N) / (100 - N);
                    }
                    else
                    {
                        pixel = (pixel * (100 - N) + 128 * N) / 100;
                    }

                    if (pixel < 0) pixel = 0;
                    if (pixel > 255) pixel = 255;

                    resultImage.setPixel(i, j, pixel);
                }
            }
            return resultImage;
        }

        public static Bitmap rotateImage(Bitmap input, float angle)
        {
            Bitmap result = new Bitmap(input.Width, input.Height);
            Graphics g = Graphics.FromImage(result);
            g.TranslateTransform((float)input.Width / 2, (float)input.Height / 2);
            g.RotateTransform(angle);
            g.TranslateTransform(-(float)input.Width / 2, -(float)input.Height / 2);
            g.DrawImage(input, new System.Drawing.Point(0, 0));
            return result;
        }

        public static Image bilinearHalfReduce(Image image)
        {
            Image resultImage = new Image(image.Width / 2, image.Height / 2, image._EdgeEffect);
            double x_koef = image.Width / (image.Width / 2);
            double y_koef = image.Height / (image.Height / 2);

            for (var i = 0; i < resultImage.Width; i++)
            {
                for (var j = 0; j < resultImage.Height; j++)
                {

                    int x = (int) x_koef * i;
                    int y = (int) y_koef * j;
                    double x_ost = (x_koef * i) - x;
                    double y_ost = (y_koef * j) - y;
                    double p1 = image.getPixel(x, y) * (1 - x_ost) * (1 - y_ost);
                    double p2 = image.getPixel(x + 1, y) * (x_ost) * (1 - y_ost);
                    double p3 = image.getPixel(x, y + 1) * (1 - x_ost) * (y_ost);
                    double p4 = image.getPixel(x + 1, y + 1) * (x_ost * y_ost);
                    resultImage.setPixel(i, j, p1 + p2 + p3 + p4);
                }
            }
            return resultImage;
        }
    }
}
