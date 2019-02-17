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
            double[,] core = new double[3, 3] {
                { koef, koef, koef},
                         { koef, koef, koef},
                         { koef, koef, koef}
            };

            return new Kernel(3, 3, core);
        }

        public static Kernel getClarity()
        {
            double[,] core = new double[3, 3]{{-1, -1, -1},
                         {-1, 9,  -1},
                         {-1, -1, -1}};
            return new Kernel(3, 3, core);
        }

        public static Kernel getSobelX()
        {
            double[,] core = new double[3, 3]{{1, 0, -1},
                         {2, 0, -2},
                         {1, 0, -1}};

            return new Kernel(3, 3, core);
        }

        public static Kernel getSobelY()
        {
            double[,] core = new double[3, 3]{{1,  2,  1},
                         {0,  0,  0},
                         {-1, -2, -1}};
            return new Kernel(3, 3, core);
        }

        public static Kernel getPriutX()
        {
            double[,] core = new double[3, 3]{{1, 0, -1},
                         {1, 0, -1},
                         {1, 0, -1}};
            return new Kernel(3, 3, core);
        }

        public static Kernel getPriutY()
        {
            double[,] core = new double[3, 3]{{1,  1,  1},
                         {0,  0,  0},
                         {-1, -1, -1}};
            return new Kernel(3, 3, core);
        }

        public static Kernel getGauss(int width, int height, double sigma)
        {
            // Tmp vars
            double sum = 0.0;
            double doubleSigma = 2 * sigma * sigma;
            double mainKoef = 1.0 / (doubleSigma * Math.PI);
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            double[,] core = new double[width, height];

            for (int i = 0; i < width; i++)
            {

                for (int j = 0; j < height; j++)
                {
                    core[i, j] = mainKoef * Math.Exp(((i - halfWidth) * (i - halfWidth) + (j - halfHeight) * (j - halfHeight)) *
                                                (-1.0 / doubleSigma));
                    sum += core[i, j];
                }
            }
            // Normalize
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    core[i, j] /= sum;
                }
            }
            return new Kernel(width, height, core);

        }
    }
}