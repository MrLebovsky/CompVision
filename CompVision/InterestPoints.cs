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
            bool [] flagUsedPoints = new bool[points.Count];

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

    }
}
