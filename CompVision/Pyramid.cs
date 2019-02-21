using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        List<Item> dogs = new List<Item>();


        public Pyramid(Image image, int scales, double sigma, double sigmaStart)
        {
            /* Reserve data */
            int octaveCount = (int)Math.Min(Math.Log(image.Width, 2), Math.Log(image.Height, 2)) - 1;

            //items.reserve(octaveCount * scales);

            /* First image */
            items.Add(new Item(convultionSeparab(image, KernelCreator.getGauss(getDeltaSigma(sigmaStart, sigma))), 0, 0, sigma, sigma));

            double sigmaScale = sigma;
            double sigmaEffect = sigma;
            double octave = 0;
            Image tmpLastImage = new Image();

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
                        tmpLastImage = ImageConverter.halfReduce(getLastImage());
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

        private static Image convultionSeparab(Image image, Kernel gaussLine) {

            ImageConverter.convolution(image, gaussLine);
            gaussLine.rotate();
            ImageConverter.convolution(image, gaussLine);

            return image;
        }

        public double getDeltaSigma(double sigmaPrev, double sigmaNext) {
            return Math.Sqrt(sigmaNext * sigmaNext - sigmaPrev* sigmaPrev);
        }

        public Image getLastImage() {
            Image res = items.ElementAt(items.Count - 1).image;
            return res;
        }
}
}
