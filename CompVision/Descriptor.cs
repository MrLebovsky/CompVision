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

            Descriptor[] descriptors = new Descriptor[interestPoints.Count];
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
        public static List<Vector> findSimilar(Descriptor[] d1, Descriptor[] d2, double treshhold)
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

        /* Поиск пика */
        private static double getPeak(double[] baskets, int notEqual = -1)
        {
            int maxBasketIndex = -1;
            for (int i = 0; i < baskets.Length; i++)
            {
                if (baskets[i] > baskets[(i - 1 + baskets.Length) % baskets.Length]
                                && baskets[i] > baskets[(i + 1) % baskets.Length] && i != notEqual)
                {
                    if (maxBasketIndex != -1 && baskets[maxBasketIndex] > baskets[i])
                    {
                        continue;
                    }
                    maxBasketIndex = i;
                }
            }
            return maxBasketIndex;
        }

        /* Интерполяция параболой */
        private static double parabaloidInterpolation(double[] baskets, int maxIndex)
        {
            // берём левую и правую корзину и интерполируем параболой
            var left = baskets[(maxIndex - 1 + baskets.Length) % baskets.Length];
            var right = baskets[(maxIndex + 1) % baskets.Length];
            var mid = baskets[maxIndex];

            var sector = 2 * Math.PI / baskets.Length;
            var phi = (left - right) / (2 * (left + right - 2 * mid));
            return (phi + maxIndex) * sector + (sector / 2);
        }

        /* Ориентация точки */
        private static double[] getPointOrientation(Image image_dx, Image image_dy, Point point, int sigma, int radius)
        {
            const int basketCount = 36;

            var dimension = radius * 2;
            var sector = 2 * Math.PI / basketCount;
            var halfSector = Math.PI / basketCount;


            double[] baskets = new double[basketCount];

            for (int i = 0; i < basketCount; i++)
                baskets[i] = 0;

            for (var i = 1; i < dimension; i++)
            {
                for (var j = 1; j < dimension; j++)
                {
                    // координаты
                    var coord_X = i - radius + point.x;
                    var coord_Y = j - radius + point.y;

                    // градиент
                    var gradient_X = image_dx.getPixel(coord_X, coord_Y);
                    var gradient_Y = image_dy.getPixel(coord_X, coord_Y);

                    // получаем значение(домноженное на Гаусса) и угол
                    var value = getGradientValue(gradient_X, gradient_Y)/* * KernelCreator::getGaussValue(i, j, sigma*2, radius)*/;
                    var phi = getGradientDirection(gradient_X, gradient_Y);

                    // получаем индекс корзины в которую входит phi и смежную с ней
                    int firstBasketIndex = (int)Math.Floor(phi / sector);
                    int secondBasketIndex = Convert.ToInt32(Math.Floor((phi - halfSector) / sector) + basketCount) % basketCount;

                    // вычисляем центр
                    var mainBasketPhi = firstBasketIndex * sector + halfSector;

                    // распределяем L(value)
                    var mainBasketValue = (1 - (Math.Abs(phi - mainBasketPhi) / sector)) * value;
                    var sideBasketValue = value - mainBasketValue;

                    // записываем значения
                    firstBasketIndex = (int)Extension.Clamp(0, basketCount - 1, firstBasketIndex);
                    secondBasketIndex = (int)Extension.Clamp(0, basketCount - 1, secondBasketIndex);
                    baskets[firstBasketIndex] += mainBasketValue;
                    baskets[secondBasketIndex] += sideBasketValue;
                }
            }

            // Ищем Пики
            var peak_1 = getPeak(baskets);
            var peak_2 = getPeak(baskets, (int)peak_1);

            //хотя бы peak_1 должен быть!

            if (peak_2 != -1 && baskets[(int)peak_2] / baskets[(int)peak_1] >= 0.8)
            { // Если второй пик не ниже 80%
                double[] peaks = new double[2];
                peaks[0] = parabaloidInterpolation(baskets, (int)peak_1);
                peaks[1] = parabaloidInterpolation(baskets, (int)peak_2);
                return peaks;
            }
            else
            {
                double[] peaks = new double[1];
                peaks[0] = parabaloidInterpolation(baskets, (int)peak_1);
                return peaks;
            }
        }

        /*  Инвариантость к вращению  */
        public static Descriptor[] getDescriptorsInvRotation(Image image, List<Point> interestPoints,
                                                            int radius, int basketCount, int barCharCount)
        {
            var sigma = 20;
            var dimension = 2 * radius;
            var sector = 2 * Math.PI / basketCount;
            var halfSector = Math.PI / basketCount;
            var barCharStep = dimension / (barCharCount / 4);
            var barCharCountInLine = (barCharCount / 4);

            Image image_dx = ImageConverter.convolution(image, KernelCreator.getSobelX());
            //image_dx.getOutputImage().Save("sobel.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

            Image image_dy = ImageConverter.convolution(image, KernelCreator.getSobelY());

            Descriptor[] descriptors = new Descriptor[interestPoints.Count];
            for (int k = 0; k < interestPoints.Count; k++)
            {
                descriptors[k] = new Descriptor(barCharCount * basketCount, interestPoints[k]);
                var peaks = getPointOrientation(image_dx, image_dy, interestPoints[k], sigma, radius);    // Ориентация точки

                foreach (var phiRotate in peaks)
                {

                    Point p = interestPoints[k];
                    p.Phi = phiRotate;
                    interestPoints[k] = p;

                    for (var i = 1; i < dimension; i++)
                    {
                        for (var j = 1; j < dimension; j++)
                        {
                            // координаты
                            var coord_X = i - radius + interestPoints[k].x;
                            var coord_Y = j - radius + interestPoints[k].y;

                            // градиент
                            var gradient_X = image_dx.getPixel(coord_X, coord_Y);
                            var gradient_Y = image_dy.getPixel(coord_X, coord_Y);

                            // получаем значение(домноженное на Гаусса) и угол
                            var value = getGradientValue(gradient_X, gradient_Y) /* * KernelCreator.getGaussValue(i, j, sigma)*/;
                            var phi = getGradientDirection(gradient_X, gradient_Y) + 2 * Math.PI - phiRotate;
                            phi = (phi % 2 * Math.PI);  // Shift

                            // получаем индекс корзины в которую входит phi и смежную с ней
                            int firstBasketIndex = (int)Math.Floor(phi / sector);
                            int secondBasketIndex = Convert.ToInt32(Math.Floor((phi - halfSector) / sector) + basketCount) % basketCount;

                            // вычисляем центр
                            var mainBasketPhi = firstBasketIndex * sector + halfSector;

                            // распределяем L(value)
                            var mainBasketValue = (1 - (Math.Abs(phi - mainBasketPhi) / sector)) * value;
                            var sideBasketValue = value - mainBasketValue;

                            // вычисляем индекс куда записывать значения
                            int i_Rotate = (int)Math.Round((i - radius) * Math.Cos(phiRotate) + (j - radius) * Math.Sin(phiRotate));
                            int j_Rotate = (int)Math.Round(-(i - radius) * Math.Sin(phiRotate) + (j - radius) * Math.Cos(phiRotate));

                            // отбрасываем
                            if (i_Rotate < -radius || j_Rotate < -radius || i_Rotate >= radius || j_Rotate >= radius)
                            {
                                continue;
                            }

                            var tmp_i = (i_Rotate + radius) / barCharStep;
                            var tmp_j = (j_Rotate + radius) / barCharStep;

                            var indexMain = (tmp_i * barCharCountInLine + tmp_j) * basketCount + firstBasketIndex;
                            var indexSide = (tmp_i * barCharCountInLine + tmp_j) * basketCount + secondBasketIndex;

                            // записываем значения
                            descriptors[k].data[indexMain] += mainBasketValue;
                            descriptors[k].data[indexSide] += sideBasketValue;
                        }
                    }
                    descriptors[k].normalize();
                    descriptors[k].clampData(0, 0.2);
                    descriptors[k].normalize();
                    descriptors[k].Point = interestPoints[k];
                }
            }
            return descriptors;
        }

        /*  Инвариантость к вращению и масштабу */
        public static Descriptor[] getDescriptorsInvRotationScale(Pyramid pyramid, List<Point> points, int _radius,
                                                                               int basketCount, int barCharCount)
        {
            var sigma = 20;
            var sigma0 = pyramid.dogs[0].sigmaScale;
            var sector = 2 * Math.PI / basketCount;
            var halfSector = Math.PI / basketCount;
            var barCharCountInLine = (barCharCount / 4);

            List<Image> images_dx = new List<Image>();
            List<Image> images_dy = new List<Image>();

            // Ищем производные заранее
            for (int i = 0; i < pyramid.dogs.Count; i++)
            {
                Image imageTrue = pyramid.dogs[i].trueImage;
                images_dx.Add(ImageConverter.convolution(imageTrue, KernelCreator.getSobelX()));
                images_dy.Add(ImageConverter.convolution(imageTrue, KernelCreator.getSobelY()));
            }

            Descriptor[] descriptors = new Descriptor[points.Count];

            for (int k = 0; k < points.Count; k++)
            {
                descriptors[k] = new Descriptor(barCharCount * basketCount, points[k]);

                double scale = (points[k].sigmaScale / sigma0);
                int radius = _radius * (int)scale;
                int dimension = 2 * radius;
                int barCharStep = dimension / (barCharCount / 4);
                Image image_dx = images_dx[points[k].z];

                //image_dx.getOutputImage().Save("sobel.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                Image image_dy = images_dy[points[k].z];
                Kernel gaussDoubleDim = KernelCreator.getGaussDoubleDim(dimension, dimension, sigma * scale);
                // Ориентация точки
                var peaks = getPointOrientation(image_dx, image_dy, points[k], sigma, radius);

                foreach (var phiRotate in peaks)
                {
                    for (var i = 1; i < dimension; i++)
                    {
                        for (var j = 1; j < dimension; j++)
                        {
                            // координаты
                            var coord_X = i - radius + points[k].x;
                            var coord_Y = j - radius + points[k].y;

                            // градиент
                            var gradient_X = image_dx.getPixel(coord_X, coord_Y);
                            var gradient_Y = image_dy.getPixel(coord_X, coord_Y);

                            // получаем значение(домноженное на Гаусса) и угол
                            var value = getGradientValue(gradient_X, gradient_Y) * gaussDoubleDim.getkernelAt(i, j);
                            //                    var value = getGradientValue(gradient_X, gradient_Y)  * KernelCreator.getGaussValue(i, j, sigma * scale, radius);
                            var phi = getGradientDirection(gradient_X, gradient_Y) + 2 * Math.PI - phiRotate;
                            phi = (phi % 2 * Math.PI);  // Shift

                            // получаем индекс корзины в которую входит phi и смежную с ней
                            int firstBasketIndex = (int)Math.Floor(phi / sector);
                            int secondBasketIndex = Convert.ToInt32(Math.Floor((phi - halfSector) / sector) + basketCount) % basketCount;

                            // вычисляем центр
                            var mainBasketPhi = firstBasketIndex * sector + halfSector;

                            // распределяем L(value)
                            var mainBasketValue = (1 - (Math.Abs(phi - mainBasketPhi) / sector)) * value;
                            var sideBasketValue = value - mainBasketValue;

                            // вычисляем индекс куда записывать значения
                            int i_Rotate = (int)Math.Round((i - radius) * Math.Cos(phiRotate) + (j - radius) * Math.Sin(phiRotate));
                            int j_Rotate = (int)Math.Round(-(i - radius) * Math.Sin(phiRotate) + (j - radius) * Math.Cos(phiRotate));

                            // отбрасываем
                            if (i_Rotate < -radius || j_Rotate < -radius || i_Rotate >= radius || j_Rotate >= radius)
                            {
                                continue;
                            }

                            var tmp_i = (i_Rotate + radius) / barCharStep;
                            var tmp_j = (j_Rotate + radius) / barCharStep;

                            var indexMain = (tmp_i * barCharCountInLine + tmp_j) * basketCount + firstBasketIndex;
                            var indexSide = (tmp_i * barCharCountInLine + tmp_j) * basketCount + secondBasketIndex;

                            // записываем значения
                            descriptors[k].data[indexMain] += mainBasketValue;
                            descriptors[k].data[indexSide] += sideBasketValue;
                        }
                    }
                    descriptors[k].normalize();
                    descriptors[k].clampData(0, 0.2);
                    descriptors[k].normalize();
                }
            }

            for (int i = 0; i < descriptors.Length; i++)
            {
                //приводим к оригинальному масштабу
                Point interPoint = descriptors[i].getInterPoint();
                double step_W = Convert.ToDouble(pyramid.dogs[0].image.Width) / pyramid.dogs[interPoint.z].image.Width;
                double step_H = Convert.ToDouble(pyramid.dogs[0].image.Height) / pyramid.dogs[interPoint.z].image.Height;
                descriptors[i].setPointXY((int)Math.Round(interPoint.x * step_W), (int)Math.Round(interPoint.y * step_H));
            }
            return descriptors;
        }


        public static Descriptor[] getDescriptorsInvRotationScaleAfinn(Pyramid pyramid, List<Point> points,
         int _radius, int basketCount, int barCharCount)
        {
            var sigma = 20;
            var sigma0 = pyramid.dogs[0].sigmaScale;
            var sector = 2 * Math.PI / basketCount;
            var halfSector = Math.PI / basketCount;
            var barCharCountInLine = (barCharCount / 4);

            List<Image> images_dx = new List<Image>();
            List<Image> images_dy = new List<Image>();

            // Ищем производные заранее
            for (int i = 0; i < pyramid.dogs.Count; i++)
            {
                Image imageTrue = pyramid.dogs[i].trueImage;
                images_dx.Add(ImageConverter.convolution(imageTrue, KernelCreator.getSobelX()));
                images_dy.Add(ImageConverter.convolution(imageTrue, KernelCreator.getSobelY()));
            }

            Descriptor[] descriptors = new Descriptor[points.Count];

            for (int k = 0; k < points.Count; k++)
            {
                descriptors[k] = new Descriptor(barCharCount * basketCount, points[k]);

                double scale = (points[k].sigmaScale / sigma0);
                int radius = _radius * (int)scale;
                int dimension = 2 * radius;
                int barCharStep = dimension / (barCharCount / 4);
                Image image_dx = images_dx[points[k].z];
                Image image_dy = images_dy[points[k].z];
                Kernel gaussDoubleDim = KernelCreator.getGaussDoubleDim(dimension, dimension, sigma * scale);
                // Ориентация точки
                var peaks = getPointOrientation(image_dx, image_dy, points[k], sigma, radius);

                foreach (var phiRotate in peaks)
                {
                    Point p = points[k];
                    p.Phi = phiRotate;
                    points[k] = p;

                    for (var i = 0; i < dimension; i++)
                    {
                        for (var j = 1; j < dimension; j++)
                        {
                            // координаты
                            var coord_X = i - radius + points[k].x;
                            var coord_Y = j - radius + points[k].y;

                            // градиент
                            var gradient_X = image_dx.getPixel(coord_X, coord_Y);
                            var gradient_Y = image_dy.getPixel(coord_X, coord_Y);

                            // получаем значение(домноженное на Гаусса) и угол
                            var value = getGradientValue(gradient_X, gradient_Y) * gaussDoubleDim.getkernelAt(i, j);
                            //                    var value = getGradientValue(gradient_X, gradient_Y)  * KernelCreator.getGaussValue(i, j, sigma * scale, radius);
                            var phi = getGradientDirection(gradient_X, gradient_Y) + 2 * Math.PI - phiRotate;
                            phi = (phi % 2 * Math.PI);  // Shift

                            // получаем индекс корзины в которую входит phi и смежную с ней
                            int firstBasketIndex = (int)Math.Floor(phi / sector);
                            int secondBasketIndex = Convert.ToInt32(Math.Floor((phi - halfSector) / sector) + basketCount) % basketCount;

                            // вычисляем центр
                            var mainBasketPhi = firstBasketIndex * sector + halfSector;

                            // распределяем L(value)
                            var mainBasketValue = (1 - (Math.Abs(phi - mainBasketPhi) / sector)) * value;
                            var sideBasketValue = value - mainBasketValue;

                            // вычисляем индекс куда записывать значения
                            int i_Rotate = (int)Math.Round((i - radius) * Math.Cos(phiRotate) + (j - radius) * Math.Sin(phiRotate));
                            int j_Rotate = (int)Math.Round(-(i - radius) * Math.Sin(phiRotate) + (j - radius) * Math.Cos(phiRotate));

                            // отбрасываем
                            if (i_Rotate < -radius || j_Rotate < -radius || i_Rotate >= radius || j_Rotate >= radius)
                            {
                                continue;
                            }

                            // координаты точки в дискрипторе
                            int true_i = (i_Rotate + radius);
                            int true_j = (j_Rotate + radius);

                            int half = (int)barCharStep / 2;

                            // отнимаем половинку для поиска ближайших 4 гистограмм
                            int disk_i = (true_i - half + dimension) % dimension;
                            int disk_j = (true_j - half + dimension) % dimension;

                            // i j левой верхней гистограммы
                            int gist_i = disk_i / (int)barCharStep;
                            int gist_j = disk_j / (int)barCharStep;

                            // индексы 4-ех гистограмм
                            int[] gist = new int[4];
                            gist[0] = gist_i * barCharCountInLine + gist_j;
                            gist[1] = gist_i + 1 >= barCharCountInLine ? -1 : (gist_i + 1) * barCharCountInLine + gist_j;
                            gist[2] = gist_j + 1 >= barCharCountInLine ? -1 : gist_i * barCharCountInLine + (gist_j + 1);
                            gist[3] = (gist_i + 1 >= barCharCountInLine || gist_j + 1 >= barCharCountInLine) ? -1 :
                                                                                    (gist_i + 1) * barCharCountInLine + (gist_j + 1);

                            // добиваемся чтоб координаты были между 4 гистограммами
                            double tmp_i = ((true_i + barCharStep / 2) % barCharStep);
                            double tmp_j = ((true_j + barCharStep / 2) % barCharStep);

                            // считаем веса  по x и y
                            double wt_X = (barCharStep - tmp_i) / barCharStep;
                            double wt_Y = (barCharStep - tmp_j) / barCharStep;

                            // перемножаем для 4 гистограмм
                            double[] wt = new double[4]{
                                wt_X * wt_Y, (1 - wt_X) * wt_Y,
                                    wt_X *(1 - wt_Y), (1 - wt_X) * (1 - wt_Y)};

                            // считаем индексы и записываем значения
                            for (int w = 0; w < 4; w++)
                            {
                                if (gist[w] != -1)
                                {
                                    int index = gist[w] * basketCount;
                                    descriptors[k].data[index + firstBasketIndex] += wt[w] * mainBasketValue;
                                    descriptors[k].data[index + secondBasketIndex] += wt[w] * sideBasketValue;
                                }
                            }
                        }
                    }
                    descriptors[k].normalize();
                    descriptors[k].clampData(0, 0.2);
                    descriptors[k].normalize();
                }
            }

            for (int i = 0; i < descriptors.Length; i++)
            {
                //приводим к оригинальному масштабу
                Point interPoint = descriptors[i].getInterPoint();
                double step_W = Convert.ToDouble(pyramid.dogs[0].image.Width) / pyramid.dogs[interPoint.z].image.Width;
                double step_H = Convert.ToDouble(pyramid.dogs[0].image.Height) / pyramid.dogs[interPoint.z].image.Height;
                descriptors[i].setPointXY((int)Math.Round(interPoint.x * step_W), (int)Math.Round(interPoint.y * step_H));
            }
            return descriptors;
        }


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

        public Point Point
        {
            get => interPoint;
            set => interPoint = value;
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

        public void setPointXY(int x, int y)
        {
            this.interPoint.x = x;
            this.interPoint.y = y;
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


