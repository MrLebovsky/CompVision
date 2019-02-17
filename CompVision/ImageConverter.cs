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

        public static void convolution(Image image, Core core)
        {
            Image copyImage = new Image(image);

            for (int i = 0; i < image.getWidth(); i++)
            {
                for (int j = 0; j < image.getHeight(); j++)
                {
                    double resultPixel = 0;
                    for (int x = 0; x < core.getWidth(); x++)
                    {
                        for (int y = 0; y < core.getHeight(); y++)
                        {
                            int realI = i + (x - (core.getWidth() / 2));
                            int realJ = j + (y - (core.getHeight() / 2));

                            resultPixel += copyImage.getPixel(realI, realJ) * core.getCoreAt(x, y);
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

            convolution(copyImageX, CoreCreator.getSobelX());
            convolution(copyImageY, CoreCreator.getSobelY());

            for (int i = 0; i < image.getWidth(); i++)
            {
                for (int j = 0; j < image.getHeight(); j++)
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
            convolution(copyImageX, CoreCreator.getPriutX());
            convolution(copyImageY, CoreCreator.getPriutY());

            for (int i = 0; i < image.getWidth(); i++)
            {
                for (int j = 0; j < image.getHeight(); j++)
                {
                    double pixelX = copyImageX.getPixel(i, j);
                    double pixelY = copyImageY.getPixel(i, j);
                    image.setPixel(i, j, Math.Sqrt(pixelX * pixelX + pixelY * pixelY));
                }
            }
        }
    }
}
