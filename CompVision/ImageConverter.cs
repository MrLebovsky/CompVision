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

        public static void convolution(Image image, Kernel kernel)
        {
            Image copyImage = new Image(image);

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

                            resultPixel += copyImage.getPixel(realI, realJ) * kernel.getCoreAt(x, y);
                        }
                    }
                    image.setPixel(i, j, resultPixel);
                }
            }

        }

        public static void sobel(Image image)
        {
            Image copyImageX = new Image(image);
            Image copyImageY = new Image(image);

            convolution(copyImageX, KernelCreator.getSobelX());
            convolution(copyImageY, KernelCreator.getSobelY());

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
            Image copyImageX = new Image(image);
            Image copyImageY = new Image(image);
            convolution(copyImageX, KernelCreator.getPriutX());
            convolution(copyImageY, KernelCreator.getPriutY());

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
    }
}
