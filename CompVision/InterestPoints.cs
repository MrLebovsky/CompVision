using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    public struct Point
    {
        public int x;
        public int y;
        public int z;
        public double s; // S(x,y) - значение оператора
        private double sigmaScale;
        private double sigmaEffect;

        public Point(int x = 0, int y = 0, double s = 0) : this()
        {
            this.x = x;
            this.y = y;
            this.s = s;
        }
        public Point(int x, int y, int z, double s, double sigmaScale = 0, double sigmaEffect = 0)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.s = s;
            this.sigmaScale = sigmaScale;
            this.sigmaEffect = sigmaEffect;
        }
    };

    public class InterestPoints
    {
        private static int Compare(Point p1, Point p2)
        {
            return p2.s.CompareTo(p1.s);
        }

        public List<Point> thresholdFilter(Image image_S, double threshold)
        {
            List<Point> points = new List<Point>();
            for (var i = 0; i < image_S.Width - 1; i++)
            {
                for (var j = 0; j < image_S.Height - 1; j++)
                {
                    if (image_S.getPixel(i, j) >= threshold)
                    {
                        points.Add(new Point(i, j, image_S.getPixel(i, j)));
                    }
                }
            }
            points.Sort(Compare);
            return points;
        }

        // Adaptive Non-Maximum Suppression
        public List<Point> anmsFilter(List<Point> points, int pointsCount)
        {
            bool[] flagUsedPoints = new bool[points.Count];

            for (int i = 0; i < points.Count; i++)
                flagUsedPoints[i] = true;

            int radius = 3;
            int usedPointsCount = points.Count;
            while (usedPointsCount > pointsCount)
            {
                for (int i = 0; i < points.Count; i++)
                {
                    if (!flagUsedPoints[i])
                    {
                        continue;
                    }

                    var p1 = points[i];
                    for (int j = i + 1; j < points.Count; j++)
                    {
                        if (flagUsedPoints[j])
                        {
                            Point p2 = points[j];
                            if (p1.s * 0.9 > p2.s && Math.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y)) <= radius)
                            {
                                flagUsedPoints[j] = false;
                                usedPointsCount--;
                                if (usedPointsCount <= pointsCount)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                radius++;
            }

            List<Point> resultPoints = new List<Point>();
            for (int i = 0; i < points.Count; i++)
            {
                if (flagUsedPoints[i])
                {
                    resultPoints.Add(points[i]);
                }
            }
            return resultPoints;
        }

        public List<Point> moravek(Image image, double threshold, int radius, int pointsCount)
        {
            Image image_S = new Image(image.Width, image.Height, image._EdgeEffect);

            for (var x = 0; x < image.Width; x++)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    double[] local_S = new double[8];                  // 8 направлений
                    for (var u = -radius; u < radius; u++)
                    {
                        for (var v = -radius; v < radius; v++)
                        {
                            double[] directDiff = new double[8];
                            var pixel = image.getPixel(x + u, y + v);
                            directDiff[0] = pixel - image.getPixel(x + u, y + v - 1);
                            directDiff[1] = pixel - image.getPixel(x + u, y + v + 1);
                            directDiff[2] = pixel - image.getPixel(x + u + 1, y + v);
                            directDiff[3] = pixel - image.getPixel(x + u + 1, y + v - 1);
                            directDiff[4] = pixel - image.getPixel(x + u + 1, y + v + 1);
                            directDiff[5] = pixel - image.getPixel(x + u - 1, y + v);
                            directDiff[6] = pixel - image.getPixel(x + u - 1, y + v - 1);
                            directDiff[7] = pixel - image.getPixel(x + u - 1, y + v + 1);

                            for (var i = 0; i < 8; i++)
                            {
                                local_S[i] += directDiff[i] * directDiff[i];
                            }
                        }
                    }
                    image_S.setPixel(x, y, local_S.Min());
                }
            }

            List<Point> points = thresholdFilter(image_S, threshold);
            return anmsFilter(points, pointsCount);
        }

        public Image HarrisMap(Image image, double threshold, int radius, int pointsCount)
        {
            Kernel gauss = KernelCreator.getGaussSlowPoke((double)radius / 3);
            image = ImageConverter.convolution(image, gauss);

            Image image_dx = ImageConverter.convolution(image, KernelCreator.getSobelX());

            Image image_dy = ImageConverter.convolution(image, KernelCreator.getSobelY());


            Image image_S = new Image(image.Width, image.Height, image._EdgeEffect);  // Веса
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image_S.setPixel(x, y, lambda(image_dx, image_dy, x, y, radius, gauss));
                }
            }

            return image_S;
        }

        public List<Point> harris(Image image, double threshold, int radius, int pointsCount)
        {
            Kernel gauss = KernelCreator.getGaussSlowPoke((double)radius / 3);
            //image = ImageConverter.convolution(image, gauss);

            Image image_dx = ImageConverter.convolution(image, KernelCreator.getSobelX());
            //ImageConverter.normolize(image_dx);

            Image image_dy = ImageConverter.convolution(image, KernelCreator.getSobelY());
            //ImageConverter.normolize(image_dy);


            Image image_S = new Image(image.Width, image.Height, image._EdgeEffect);  // Веса
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image_S.setPixel(x, y, lambda(image_dx, image_dy, x, y, radius, gauss));
                }
            }

            List<Point> localMaximumPoints = localMaximum(thresholdFilter(image_S, threshold), image_S); 
            return anmsFilter(localMaximumPoints, pointsCount); 
        }

        public double lambda(Image image_dx, Image image_dy, int x, int y, int radius, Kernel gauss)
        {
            double A = 0, B = 0, C = 0;
            int k = 0, q = 0;

            for (var i = x - radius; i < x + radius; i++)
            {
                for (var j = y - radius; j < y + radius; j++)
                {
                    //Вычисляем матрицу H
                    var curA = image_dx.getPixel(i, j);
                    var curB = image_dy.getPixel(i, j) ;
                    A += curA * curA * gauss.getkernelAt(q, k);
                    B += curA * curB * gauss.getkernelAt(q, k);
                    C += curB * curB * gauss.getkernelAt(q, k);
                    k++;
                }
                k = 0;
                q++;
            }
            //var descreminant = Math.Sqrt((C - A) * (C - A) + 4 * B * B);

            return ((A * C - B * B) - 0.05 * (A + C) * (A + C)); //вариант оригинального Харриса
        }

        public List<Point> localMaximum(List<Point> points, Image image_S)
        {
            List<Point> result = new List<Point>();
            const int radius = 2;

            //Смотрим интенсивность всех точек в окрестности
            //c заданным радиусом
            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                bool flagMaximum = true;

                for (var j = -radius; j <= radius; ++j)
                {
                    for (var k = -radius; k <= radius; ++k)
                    {
                        if (j == 0 && k == 0)
                        {
                            continue;
                        }

                        if (image_S.getPixel(p1.x + j, p1.y + k) >= p1.s)
                        {
                            flagMaximum = false;
                            break;
                        }
                    }
                }

                if (flagMaximum == true)
                {
                    result.Add(p1);
                }
            }
            return result;
        }

        public Image Canny(Image image)
        {
            image = Pyramid.convultionSeparab(image, KernelCreator.getGauss(1));

            Image Gx = ImageConverter.convolution(image, KernelCreator.getSobelX());
            Image Gy = ImageConverter.convolution(image, KernelCreator.getSobelY());
            ImageConverter.sobel(image);
            Image Gradient = image;

            for (var i = 0; i < image.Width; i++)
            {
                for (var j = 0; j < image.Height; j++)
                {
                    //Вычислим направление градиента
                    double t = (Math.Atan(Gx.getPixel(i, j)/ Gy.getPixel(i, j)) * 180 / Math.PI);

                    //Horizontal Edge
                    if (((-22.5 < t) && (t <= 22.5)) || ((157.5 < t) && (t <= -157.5)))
                    {
                        if ((Gradient.getPixel(i, j) < Gradient.getPixel(i, j + 1)) || (Gradient.getPixel(i, j)
                                                                                        < Gradient.getPixel(i, j - 1)))
                            Gradient.setPixel(i, j, 0);
                    }
                    //Vertical Edge
                    if (((-112.5 < t) && (t <= -67.5)) || ((67.5 < t) && (t <= 112.5)))
                    {
                        if ((Gradient.getPixel(i, j) < Gradient.getPixel(i + 1, j)) || (Gradient.getPixel(i, j) < Gradient.getPixel(i - 1, j)))
                            Gradient.setPixel(i, j, 0);
                    }
                    //+45 Degree Edge
                    if (((-67.5 < t) && (t <= -22.5)) || ((112.5 < t) && (t <= 157.5)))
                    {
                        if ((Gradient.getPixel(i, j) < Gradient.getPixel(i + 1, j - 1)) || (Gradient.getPixel(i, j) < Gradient.getPixel(i - 1, j + 1)))
                            Gradient.setPixel(i, j, 0);
                    }
                    //-45 Degree Edge
                    if (((-157.5 < t) && (t <= -112.5)) || ((67.5 < t) && (t <= 22.5)))
                    {
                        if ((Gradient.getPixel(i, j) < Gradient.getPixel(i + 1, j + 1)) || (Gradient.getPixel(i, j) < Gradient.getPixel(i - 1, j - 1)))
                            Gradient.setPixel(i, j, 0);
                    }   
                }
            }

            double DownThreshold = 110;
            double UpThreshold = 120;

            //Двойная пороговая фильтрация
            for (var i = 0; i < image.Width; i++)
            {
                for (var j = 0; j < image.Height; j++)
                {
                    if (Gradient.getPixel(i, j) <= DownThreshold)
                    {
                        Gradient.setPixel(i, j, 0);
                    }
                    else if (Gradient.getPixel(i, j) >= UpThreshold)
                    {
                        Gradient.setPixel(i, j, 255);
                    }
                    else Gradient.setPixel(i, j, 127);
                }
            }
            return image;
        }
    }
}
