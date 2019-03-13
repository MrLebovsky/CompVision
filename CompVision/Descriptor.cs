using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompVision
{
    public static class Extension
    {
        public static double Clamp(double min, double max, double value)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }

    public class DescriptorCreator
    {
        public static Descriptor[] getDescriptors(Image image, List<Point> interestPoints, int radius, int basketCount, int barCharCount)
        {
            var dimension = 2 * radius; //размерность окрестности интересной точки
            var sector = 2 * Math.PI / basketCount; //размер одной корзины в гистограмме
            var halfSector = Math.PI / basketCount; // размер половины одной корзины в гистограмме
            var barCharStep = dimension / (barCharCount / 4); //шаг гистограммы
            var barCharCountInLine = (barCharCount / 4);


            Image image_dx = ImageConverter.convolution(image, KernelCreator.getSobelX());
            Image image_dy = ImageConverter.convolution(image, KernelCreator.getSobelY());

            Descriptor [] descriptors = new Descriptor[interestPoints.Count];
            for (int k = 0; k < interestPoints.Count; k++)
            {
                descriptors[k] = new Descriptor(barCharCount * basketCount, interestPoints[k]);

                for (var i = 0; i < dimension; i++)
                {
                    for (var j = 0; j < dimension; j++)
                    {
                        // get Gradient
                        var gradient_X = image_dx.getPixel(i - radius + interestPoints[k].x, j - radius + interestPoints[k].y);
                        var gradient_Y = image_dy.getPixel(i - radius + interestPoints[k].x, j - radius + interestPoints[k].y);

                        // get value and phi
                        var value = getGradientValue(gradient_X, gradient_Y);
                        var phi = getGradientDirection(gradient_X, gradient_Y);

                        // получаем индекс корзины в которую входит phi и смежную с ней
                        int firstBasketIndex = (int)Math.Floor(phi / sector);
                        int secondBasketIndex = (int)(Math.Floor((phi - halfSector) / sector) + basketCount) % basketCount;

                        // вычисляем центр
                        var mainBasketPhi = firstBasketIndex * sector + halfSector;

                        // распределяем L(value)
                        var mainBasketValue = (1 - (Math.Abs(phi - mainBasketPhi) / sector)) * value;
                        var sideBasketValue = value - mainBasketValue;

                        // вычисляем индекс куда записывать значения
                        var tmp_i = i / barCharStep;
                        var tmp_j = j / barCharStep;

                        var indexMain = (tmp_i * barCharCountInLine + tmp_j) * basketCount + firstBasketIndex;
                        var indexSide = (tmp_i * barCharCountInLine + tmp_j) * basketCount + secondBasketIndex;

                        if (indexMain >= descriptors[k].data.Length)
                            indexMain = 0;

                        if (indexSide >= descriptors[k].data.Length)
                            indexSide = 0;

                        // записываем значения
                        descriptors[k].data[indexMain] += mainBasketValue;
                        descriptors[k].data[indexSide] += sideBasketValue;
                    }
                }
                descriptors[k].normalize();
                descriptors[k].clampData(0, 0.2);
                descriptors[k].normalize();
            }
            return descriptors;
        }

        // Поиск похожих дескрипторов
        public static List<Vector> findSimilar(Descriptor []d1, Descriptor []d2, double treshhold)
        {
            List<Vector> similar = new List<Vector>();
            for (int i = 0; i < d1.Length; i++)
            {
                int indexSimilar = -1;
                double prevDistance = Double.MaxValue;       // Предыдущий
                double minDistance = Double.MaxValue;        // Минимальный
                for (int j = 0; j < d2.Length; j++)
                {
                    double dist = getDistance(d1[i], d2[j]);
                    if (dist < minDistance)
                    {
                        indexSimilar = j;
                        prevDistance = minDistance;
                        minDistance = dist;
                    }
                }

                if (minDistance / prevDistance > treshhold)
                {
                    continue;      // отбрасываем
                }
                else
                {
                    similar.Add(new Vector(d1[i], d2[indexSimilar]));
                }
            }
            return similar;
        }

        private static double getDistance(Descriptor d1, Descriptor d2)
        {
            double result = 0;
            for (int i = 0; i < d1.data.Length; i++)
            {
                    double tmp = d1.data[i] - d2.data[i];
                    result += tmp * tmp;
            }
            return Math.Sqrt(result);
        }

        private static double getGradientValue(double x, double y)
        {
            return Math.Sqrt(x * x + y * y);
        }

        private static double getGradientDirection(double x, double y) { return Math.Atan2(y, x) + Math.PI; } //угол градиента в радианах
    }

    public class Descriptor
    {
        private Point interPoint;    // Интересная точка - центр
        public double[] data; // N - Количество корзин * L кол-во гистограмм

        public Descriptor() { }

        public Descriptor(int size, Point interPoint)
        {
            data = new double[size];
            this.interPoint = interPoint;
        }

        public void normalize()
        {
            double length = 0;
            foreach (double a in data)
                length += a * a;

            length = Math.Sqrt(length);

            for (int i = 0; i < data.Length; i++)
                data[i] /= length;
        }

        public int getSize
        {
            get => data.Length;
        }

        public double getAt(int index) { return data[index]; }

        public Point getInterPoint() { return interPoint; }

        public void clampData(double min, double max)
        {
            for (int i = 0; i < data.Length; i++)
                data[i] = Extension.Clamp(min, max, data[i]);
        }
    }

    public struct Vector
    {
        public Descriptor first;
        public Descriptor second;

        public Vector(Descriptor first, Descriptor second)
        {
            this.first = first;
            this.second = second;
        }
    }
}


