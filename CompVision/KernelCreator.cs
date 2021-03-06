﻿using System;
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
            //int radius = (int) sigma * 6;
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

        public static Kernel getGaussSlowPoke(double sigma)
        {
            double sum = 0.0;
            double doubleSigma = 2 * sigma * sigma;
            double mainKoef = 1.0 / doubleSigma * Math.PI;
            
            int size = (int)(2 * (3 * sigma) + 1);

            double halfWidth = size / 2;
            double halfHeight = size / 2;

            double[] kernel = new double[size * size];

            Parallel.For(0, size, i =>
            {

                for (int j = 0; j < size; j++)
                {
                    kernel[i + j * size] = mainKoef * Math.Exp((-((i - halfWidth) * 
                        (i - halfWidth) + (j - halfHeight) * (j - halfHeight)) /(doubleSigma)));

                    sum += kernel[i + j * size];
                }
            });

            // Normalize
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i + j * size] /= sum;
                }
            }

            return new Kernel(size, size , kernel);
        }

        public static Kernel getShift()
        {
            double[] kernel = new double[9]{0, 1, 0,
                                             0, 0, 0,
                                             0, 0, 0};
            return new Kernel(3, 3, kernel);
        }

        public static double getGaussValue(int i, int j, double sigma)
        {
            int radius = (int)(sigma * 6);
            if (radius % 2 == 0) radius++;
            return (1.0 / (2 * sigma * sigma * Math.PI)) * Math.Exp(((i - radius / 2) 
                * (i - radius / 2) + (j - radius / 2) * (j - radius / 2)) * (-1.0 / (2 * sigma * sigma)));
        }

        public static double getGaussValue(int i, int j, double sigma, int radius) {
            return (1.0 / (2 * sigma* sigma * Math.PI)) 
                * Math.Exp(((i - radius) * (i - radius) + (j - radius) * (j - radius)) * (-1.0 / (2 * sigma* sigma)));
    }

    public static Kernel getGaussDoubleDim(int width, int height, double sigma)
        {
            // Tmp vars
            double sum = 0.0;
            double doubleSigma = 2 * sigma * sigma;
            double mainKoef = 1.0 / (doubleSigma * Math.PI);
            double halfWidth = width / 2;
            double halfHeight = height / 2;

            double [] core = new double[width* height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    core[i + j * width] =
                            mainKoef * Math.Exp(((i - halfWidth) * (i - halfWidth) + (j - halfHeight) * (j - halfHeight)) *
                                           (-1.0 / doubleSigma));
                    sum += core[i + j * width];
                }
            }

            // Normalize
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    core[i + j * width] /= sum;
                }
            }
            return new Kernel(width, height, core);
        }
    }
}