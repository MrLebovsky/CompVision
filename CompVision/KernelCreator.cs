using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    class KernelCreator
    {
        public KernelCreator() { }

        public static Kernel getBlur()
        {
            const double koef = 1.0 / 9;
            double[] kernel = new double[9] {
                          koef, koef, koef,
                          koef, koef, koef,
                          koef, koef, koef
            };

            return new Kernel(3, 3, kernel);
        }

        public static Kernel getClarity()
        {
            double[] kernel = new double[9]{-1, -1, -1,
                                             -1, 9,  -1,
                                             -1, -1, -1};
            return new Kernel(3, 3, kernel);
        }

        public static Kernel getSobelX()
        {
            double[] kernel = new double[9]{1, 0, -1,
                                            2, 0, -2,
                                            1, 0, -1};

            return new Kernel(3, 3, kernel);
        }

        public static Kernel getSobelY()
        {
            double[] kernel = new double[9]{1,  2,  1,
                                             0,  0,  0,
                                             -1, -2, -1};
            return new Kernel(3, 3, kernel);
        }

        public static Kernel getPriutX()
        {
            double[] kernel = new double[9]{1, 0, -1,
                                             1, 0, -1,
                                             1, 0, -1};
            return new Kernel(3, 3, kernel);
        }

        public static Kernel getPriutY()
        {
            double[] kernel = new double[9]{1,  1,  1,
                                             0,  0,  0,
                                             -1, -1, -1};
            return new Kernel(3, 3, kernel);
        }

        public static Kernel getGauss(double sigma)
        {
            int radius = (int)(2 * (3 * sigma) + 1);
            if (radius % 2 == 0)
            {
                radius++;
            }
            double sum = 0;
            double doubleSigma = 2 * sigma * sigma;
            double mainKoef = 1 / (Math.Sqrt(2 * Math.PI) * sigma);
            double []core = new double[radius];

            for (int i = 0; i < radius; i++)
            {
                core[i] = mainKoef * Math.Exp(-(Math.Pow(i - (radius / 2), 2)) / doubleSigma);
                sum += core[i];
            }

            for (int i = 0; i < radius; i++)
            {
                core[i] /= sum;
            }
            return new Kernel(1, radius, core);
        }

    }
}