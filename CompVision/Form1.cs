using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompVision
{
    public partial class Form1 : Form
    {
        private Image image;
        private Bitmap inputImage;
        private Image.EdgeEffect edgeEffect;
        private String filePath;

        public Form1()
        {
            InitializeComponent();
            filePath = @"C:\Users\Роман\source\repos\CompVision\CompVision\Res\Shrikrishna.bmp";
            inputImage = new Bitmap(filePath);
            image = new Image(inputImage, edgeEffect);
            edgeEffect = Image.EdgeEffect.Black;
            SetImage(image, image.Width, image.Height);           
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void SetImage(Image xImage, int xSize, int ySize)
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
            SetImage(image, image.Width, image.Height);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImageConverter.sobel(image);
            SetImage(image, image.Width, image.Height);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImageConverter.convolution(image, KernelCreator.getGauss(5, 5, 0.5));
            SetImage(image, image.Width, image.Height);
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
            ImageConverter.convolution(image, KernelCreator.getBlur());
            SetImage(image, image.Width, image.Height);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ImageConverter.convolution(image, KernelCreator.getClarity());
            SetImage(image, image.Width, image.Height);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ImageConverter.priut(image);
            SetImage(image, image.Width, image.Height);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetOldImage();
            this.Size = new System.Drawing.Size(1432, 520);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Size = new System.Drawing.Size(846, 520);
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
            SetImage(image, image.Width, image.Height);
        }
    }
}
