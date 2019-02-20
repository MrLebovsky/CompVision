# CompVision
            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    double pixel = (image.getPixel(i, j) - min) * (255/(max-min));

                    image.setPixel(i, j, pixel);
                }
            }

![Иллюстрация к проекту](https://github.com/MrLebovsky/CompVision/blob/master/CompVision/61_tn.png)
