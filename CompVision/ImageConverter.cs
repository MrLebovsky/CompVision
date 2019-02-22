using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CompVision
{
    class ImageConverter
    {
        ImageConverter() { }

        public static Image convolution(Image image, Kernel kernel)
        {
            Image resultImage = new Image(image.Width, image.Height, image._EdgeEffect);

            for(int i =0; i < image.Width; i++)
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

    }
}
