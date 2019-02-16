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

        public Form1()
        {
            InitializeComponent();
            inputImage = new Bitmap(@"C:\Users\Роман\source\repos\CompVision\CompVision\Res\Shrikrishna.bmp");
            image = new Image(inputImage);
            SetImage(image, 576, 432);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        public void SetImage(Image xImage, int xSize, int ySize)
        {
            // Stretches the image to fit the pictureBox.
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            image = new Image(xImage);
            pictureBox1.ClientSize = new Size(xSize, ySize);
            pictureBox1.Image = image.getOutputImage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            image = new Image(inputImage);
            SetImage(image, 576, 432);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ImageConverter.sobel(image);
            SetImage(image, 576, 432);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ImageConverter.convolution(image, CoreCreator.getGauss(5, 5, 0.5));
            SetImage(image, 576, 432);
        }
    }
}
