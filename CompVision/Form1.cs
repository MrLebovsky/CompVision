using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CompVision
{
    public partial class Form1 : Form
    {
        private Image image;

        public Image First;
        public Image Second;

        private Bitmap inputImage;
        private Image.EdgeEffect edgeEffect;
        private String filePath;
        private int curPyramidIdex = 0;
        private Pyramid pyramid;
        InterestPoints interestPoints;

        public Form1()
        {
            InitializeComponent();
            filePath = @"C:\Users\Роман\source\repos\CompVision\CompVision\Res\lenna.jpg";
            inputImage = new Bitmap(filePath);
            edgeEffect = Image.EdgeEffect.Repeat;
            image = new Image(inputImage, edgeEffect);
            this.radioButton2.Checked = true;
            SetImage(image);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void SetImage(Image xImage)
        {
            // Stretches the image to fit the pictureBox.
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            image = new Image(xImage);
            pictureBox1.Image = image.getOutputImage();
        }

        public void SetOldImage()
        {
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.Image = image.getOutputImage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            image = new Image(inputImage, edgeEffect);
            SetImage(image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImageConverter.sobel(image);
            SetImage(image);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            image = Pyramid.convultionSeparab(image, KernelCreator.getGauss(Convert.ToDouble(textBox1.Text)));
            ImageConverter.normolize(image);
            SetImage(image);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            image._EdgeEffect = Image.EdgeEffect.Wrapping;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            image._EdgeEffect = Image.EdgeEffect.Mirror;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            image._EdgeEffect = Image.EdgeEffect.Repeat;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            image._EdgeEffect = Image.EdgeEffect.Black;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            image = ImageConverter.convolution(image, KernelCreator.getBlur());
            ImageConverter.normolize(image);
            SetImage(image);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            image = ImageConverter.convolution(image, KernelCreator.getClarity());
            SetImage(image);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ImageConverter.priut(image);
            SetImage(image);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetOldImage();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //this.Size = new System.Drawing.Size(846, 520);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void загрузитьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog() { Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*" };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                filePath = openFileDialog1.FileName;

            inputImage = new Bitmap(filePath);
            image = new Image(inputImage, edgeEffect);
            SetImage(image);
        }

        private void getSobelXToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void getSobelYToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            image = ImageConverter.convolution(image, KernelCreator.getSobelX());
            //ImageConverter.normolize(image);
            SetImage(image);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            image = ImageConverter.convolution(image, KernelCreator.getSobelY());
            //ImageConverter.normolize(image);
            SetImage(image);
        }

        public void ShowPyramidInfo(Item item)
        {
            richTextBox1.Text =
               "Octave:     " + item.octave + "  from  " + pyramid.octaveSize + "\n" +
               "Scale:     " + item.scale + "\n" +
               "SigmaScale:     " + item.sigmaScale + "\n" +
               "SigmaEffect:     " + item.sigmaEffect;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            pyramid = new Pyramid(image, Convert.ToInt32(textBox2.Text), Convert.ToDouble(textBox3.Text),
                            Convert.ToDouble(textBox4.Text));

            curPyramidIdex = 0;

            SetImage(pyramid.items.ElementAt(curPyramidIdex).image);
            ShowPyramidInfo(pyramid.items.ElementAt(curPyramidIdex));
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (curPyramidIdex > 0)
            {
                curPyramidIdex--;
            }
            SetImage(pyramid.items.ElementAt(curPyramidIdex).image);
            ShowPyramidInfo(pyramid.items.ElementAt(curPyramidIdex));
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (curPyramidIdex < pyramid.items.Count - 1)
            {
                curPyramidIdex++;
            }
            SetImage(pyramid.items.ElementAt(curPyramidIdex).image);
            ShowPyramidInfo(pyramid.items.ElementAt(curPyramidIdex));
        }

        private void button14_Click(object sender, EventArgs e)
        {
            int i = pyramid.L(Convert.ToDouble(textBox5.Text));
            Bitmap result = Image.createFromIndex(image, i, pyramid);
            pictureBox1.Image = result;
            ShowPyramidInfo(pyramid.items.ElementAt(i));
        }

        private void button15_Click(object sender, EventArgs e)
        {
            var blur = new GaussianBlur(image.getOutputImage());
            var result = blur.Process(Convert.ToInt32(textBox1.Text));
            image = new Image(result, edgeEffect);
            ImageConverter.normolize(image);
            SetImage(image);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            interestPoints = new InterestPoints();
            image.normolizePixels();
            List<Point> points = interestPoints.moravek(image, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);
            pictureBox1.Image = Image.createImageWithPoints(image, points);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            String path = "";
            using (var dialog = new FolderBrowserDialog())
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.SelectedPath;
                    pyramid.SavePyramidToFile(path);
                }



        }

        private void button18_Click(object sender, EventArgs e)
        {
            interestPoints = new InterestPoints();
            image.normolizePixels();
            List<Point> points = interestPoints.harris(image, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);
            pictureBox1.Image = Image.createImageWithPoints(image, points);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            interestPoints = new InterestPoints();
            image = interestPoints.Canny(image);
            SetImage(image);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            image = ImageConverter.convolution(image, KernelCreator.getShift());
            SetImage(image);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            image = ImageConverter.noise(image, 2000);
            SetImage(image);
        }

        private void button22_Click(object sender, EventArgs e)
        {
            image = ImageConverter.rotate(image);
            SetImage(image);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            image = ImageConverter.Brightness(image, trackBar1.Value, trackBar1.Maximum);
            SetImage(image);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            image = ImageConverter.Сontrast(image, trackBar2.Value, trackBar2.Maximum);
            SetImage(image);
        }

        private void button23_Click(object sender, EventArgs e)
        {
            InterestPoints interestPoints = new InterestPoints();
            image.normolizePixels();
            image = interestPoints.HarrisMap(image, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);
            SetImage(image);
        }

        private void button24_Click(object sender, EventArgs e)
        {
            interestPoints = new InterestPoints();
            Image FirstImage = new Image(First);
            FirstImage.normolizePixels();
            List<Point> FirstPoints = interestPoints.harris(FirstImage, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);

            Descriptor[] descriptors1 = DescriptorCreator.getDescriptors(First, FirstPoints, (int)numericUpDown6.Value,
                (int)numericUpDown4.Value, (int)numericUpDown5.Value);


            Image SecondImage = new Image(Second);
            SecondImage.normolizePixels();
            List<Point> SecondPoints = interestPoints.harris(SecondImage, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);

            Descriptor[] descriptors2 = DescriptorCreator.getDescriptors(Second, SecondPoints, (int)numericUpDown6.Value,
                (int)numericUpDown4.Value, (int)numericUpDown5.Value);

            Bitmap res = Image.glueImages(Image.createImageWithPoints(FirstImage, FirstPoints),
                Image.createImageWithPoints(SecondImage, SecondPoints));

            List<Vector> similar = DescriptorCreator.findSimilar(descriptors1, descriptors2, (double)numericUpDown7.Value);
            Image.drawLines(res, First.Width, similar);

            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = res;
        }

        private void button25_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog() { Filter = 
                "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*" };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                filePath = openFileDialog1.FileName;

            First = new Image(new Bitmap(filePath), edgeEffect);
            SetImage(First);
        }

        private void button26_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog() { Filter = 
                "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG|All files (*.*)|*.*" };
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                filePath = openFileDialog1.FileName;

            Second = new Image(new Bitmap(filePath), edgeEffect);
            SetImage(Second);
        }

        private void button27_Click(object sender, EventArgs e)
        {
            First = new Image(ImageConverter.rotateImage(First.getOutputImage(), 
                float.Parse(textBox6.Text)), edgeEffect);
            SetImage(First);
        }

        private void button28_Click(object sender, EventArgs e)
        {
            Second = new Image(ImageConverter.rotateImage(Second.getOutputImage(), 
                float.Parse(textBox6.Text)), edgeEffect);
            SetImage(Second);
        }

        private void button29_Click(object sender, EventArgs e)
        {
            interestPoints = new InterestPoints();
            Image FirstImage = new Image(First);
            FirstImage.normolizePixels();
            List<Point> FirstPoints = interestPoints.harris(FirstImage, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);

            Descriptor[] descriptors1 = DescriptorCreator.getDescriptorsInvRotation(First, FirstPoints, (int)numericUpDown6.Value,
                (int)numericUpDown4.Value, (int)numericUpDown5.Value);


            Image SecondImage = new Image(Second);
            SecondImage.normolizePixels();
            List<Point> SecondPoints = interestPoints.harris(SecondImage, (double)numericUpDown1.Value,
                            (int)numericUpDown2.Value,
                            (int)numericUpDown3.Value);

            Descriptor[] descriptors2 = DescriptorCreator.getDescriptorsInvRotation(Second, SecondPoints, (int)numericUpDown6.Value,
                (int)numericUpDown4.Value, (int)numericUpDown5.Value);

            Bitmap res = Image.glueImages(Image.createImageWithPoints(FirstImage, FirstPoints),
                Image.createImageWithPoints(SecondImage, SecondPoints));

            List<Vector> similar = DescriptorCreator.findSimilar(descriptors1, descriptors2, (double)numericUpDown7.Value);
            Image.drawLines(res, First.Width, similar);

            this.pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.Image = res;
        }

        private void сохранитьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) //если в pictureBox есть изображение
            {
                //создание диалогового окна "Сохранить как..", для сохранения изображения
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Title = "Сохранить картинку как...";
                //отображать ли предупреждение, если пользователь указывает имя уже существующего файла
                savedialog.OverwritePrompt = true;
                //отображать ли предупреждение, если пользователь указывает несуществующий путь
                savedialog.CheckPathExists = true;
                //список форматов файла, отображаемый в поле "Тип файла"
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
                //отображается ли кнопка "Справка" в диалоговом окне
                savedialog.ShowHelp = true;
                if (savedialog.ShowDialog() == DialogResult.OK) //если в диалоговом окне нажата кнопка "ОК"
                {
                    try
                    {
                        pictureBox1.Image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
