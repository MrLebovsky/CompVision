using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CompVision
{
    public struct Item
    {
        public int octave;
        public double scale;
        public double sigmaScale;
        public double sigmaEffect;
        public Image image;
        public Image trueImage;

        public Item(Image iImage, int iOctave, double iScale, double iSigmaScale, double iSigmaEffect) : this()
        {
            octave = iOctave;
            scale = iScale;
            sigmaScale = iSigmaScale;
            sigmaEffect = iSigmaEffect;
            image = iImage;
        }

        public Item(Image itrueImage, Image iimage, int ioctave, double iscale, double isigmaScale, double isigmaEffect)
        {
            octave = ioctave;
            scale = iscale;
            sigmaScale = isigmaScale;
            sigmaEffect = isigmaEffect;
            image =iimage;
            trueImage = itrueImage;
        }
    };

    public class Pyramid
    {
        public List<Item> items = new List<Item>();
        public List<Item> dogs = new List<Item>();
        public int octaveSize;
        public int ScalesCount;

        public Pyramid() { }

        public Pyramid(Image image, int scales = 7, double sigma = 1.6, double sigmaStart = 1, int t = 0)
        {
            /* Reserve data */
            int octaveCount = (int)Math.Min(Math.Log(image.Width, 2), Math.Log(image.Height, 2)) - 1;

            /* First image */
            items.Add(new Item(convultionSeparab(image, KernelCreator.getGauss(getDeltaSigma(sigmaStart, sigma))), 0, 0, sigma, sigma));

            double sigmaScale = sigma;
            double sigmaEffect = sigma;
            double octave = 0;
            Image tmpLastImage = null;

            // While image can be reduced
            while (octaveCount > 0)
            {
                double intervalSigma = Math.Pow(2, 1.0 / scales);

                for (int i = 0; i < scales + 3; i++)
                {
                    double sigmaScalePrev = sigmaScale;
                    sigmaScale = sigma * Math.Pow(intervalSigma, i + 1);
                    double deltaSigma = getDeltaSigma(sigmaScalePrev, sigmaScale);
                    sigmaEffect = sigmaScale * Math.Pow(2, octave);

                    items.Add(new Item(convultionSeparab(getLastImage(), KernelCreator.getGauss(deltaSigma)), (int)octave, i + 1,
                                      sigmaScale, sigmaEffect));

                    if (i == scales - 1)
                    {
                        tmpLastImage = ImageConverter.bilinearHalfReduce(getLastImage());
                    }
                }
                octave++;
                sigmaEffect = sigma * Math.Pow(2, octave);
                sigmaScale = sigma;
                octaveCount--;

                items.Add(new Item(tmpLastImage, (int)octave, 0, sigmaScale, sigmaEffect));
            }

            /* Constructs DOGs */
            for (int i = 1; i < items.Count; i++)
            {
                if (Image.sizeEq(items[i - 1].image, items[i].image))
                {
                    Item item = items[i - 1];
                    Item dog = new Item(items[i].image, items[i].image - item.image,
                                  item.octave, item.scale, item.sigmaScale, item.sigmaEffect);
                    dogs.Add(dog);
                }
            }
        }

        /*
        public Pyramid(Image image, int scales, double sigma, double sigmaStart)
        {
            /* Reserve data 
            int octaveCount = (int)Math.Min(Math.Log(image.Width, 2), Math.Log(image.Height, 2)) - 1;
            octaveSize = octaveCount;
            ScalesCount = scales;
            //items.reserve(octaveCount * scales);

            /* First image 
            items.Add(new Item(convultionSeparab(image, KernelCreator.getGauss(getDeltaSigma(sigmaStart, sigma))), 0, 0, sigma, sigma));

            double sigmaScale = sigma;
            double sigmaEffect = sigma;
            double octave = 0;
            Image tmpLastImage = new Image();

            // While image can be reduced
            while (octaveCount > 0)
            {
                double intervalSigma = Math.Pow(2, 1.0 / scales);

                for (int i = 0; i < scales; i++)
                {
                    double sigmaScalePrev = sigmaScale;
                    sigmaScale = sigma * Math.Pow(intervalSigma, i + 1);
                    double deltaSigma = getDeltaSigma(sigmaScalePrev, sigmaScale);
                    sigmaEffect = sigmaScale * Math.Pow(2, octave);

                    if (i == scales - 1)
                    {
                        tmpLastImage = ImageConverter.halfReduce(getLastImage());
                        items.Add(new Item(tmpLastImage, (int)octave, 0, sigmaScale, sigmaEffect));
                    }
                    else {
                        items.Add(new Item(convultionSeparab(getLastImage(), KernelCreator.getGauss(deltaSigma)), (int)octave, i + 1,
                                      sigmaScale, sigmaEffect));
                    }
                }
                octave++;
                sigmaEffect = sigma * Math.Pow(2, octave);
                sigmaScale = sigma;
                octaveCount--;

                //items.Add(new Item(tmpLastImage, (int)octave, 0, sigmaScale, sigmaEffect));
            }

            /* Constructs DOGs 
            for (int i = 1; i < items.Count; i++)
            {
                if (Image.sizeEq(items[i - 1].image, items[i].image))
                {
                    Item item = items[i - 1];
                    Item dog = new Item(items[i].image, items[i].image - item.image,
                                      item.octave, item.scale, item.sigmaScale, item.sigmaEffect);
                    dogs.Add(dog);
                }
            }
        }
    */

        public static Image convultionSeparab(Image image, Kernel gaussLine)
        {

            image = ImageConverter.convolution(image, gaussLine);
            gaussLine.rotate();

            return ImageConverter.convolution(image, gaussLine);
        }

        public double getDeltaSigma(double sigmaPrev, double sigmaNext)
        {
            return Math.Sqrt(sigmaNext * sigmaNext - sigmaPrev * sigmaPrev);
        }

        public Image getLastImage()
        {
            return new Image(items.ElementAt(items.Count - 1).image);
        }

        //https://math.stackexchange.com/a/447670
        public int nearestGeometricProgressionElement(double a, double q, double value)
        {
            double y = (Math.Log(value) - Math.Log(a)) / Math.Log(q);
            return (int)Math.Round(y);
        }

        public int L(double sigma)
        {
            if (items.Count > 0)
            {
                int i = Math.Max(0, nearestGeometricProgressionElement(items[0].sigmaEffect, Math.Pow(2, (double)1 / ScalesCount), sigma));
                if (i > items.Count - 1) i = items.Count - 1;
                int octave = i / ScalesCount; //какую октаву взяли
                int subIndex = i % ScalesCount; //номер масштаба в октаве

                return i;
            }
            return 0;
            
        }

        public double CoordinateTransform(int x, int y, int index)
        {
            int width = items[0].image.Width;
            int height = items[0].image.Height;
            Image outPut = items[index].image;


            int xCur = (int)(x * outPut.Width * (1.0 / width));
            int yCur = (int)(y * outPut.Height * (1.0 / height));
            return (int)outPut.getPixel(xCur, yCur);
        }

        public void SavePyramidToFile(String path)
        {
            if (items.Count > 0)
            {
                for (int i = 0; i < items.Count - 1; i++)
                {
                    String fullPath = path + "\\Octave " + items[i].octave.ToString() + " Scale " + items[i].scale.ToString()
                        + " SigmaEffect " + items[i].sigmaEffect.ToString() + " "
                        + ".jpeg";

                    items[i].image.getOutputImage().Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }
    }
    
}
