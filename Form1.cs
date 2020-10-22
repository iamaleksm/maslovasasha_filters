using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WindowsFormsApp2.Filters;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        Bitmap image, startImage, nowImage;
        List<Bitmap> list;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void назадToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void впередToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void кнопкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            image = startImage;
            pictureBox1.Image = image;
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.png;*.jpg;*.bmp|All files(*.*)|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                startImage = image;
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.Refresh();

            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) 
            {
                
                SaveFileDialog savedialog = new SaveFileDialog();
                savedialog.Filter = "Image Files(*.BMP)|*.BMP|Image Files(*.JPG)|*.JPG|Image Files(*.GIF)|*.GIF|Image Files(*.PNG)|*.PNG|All files (*.*)|*.*";
               
                if (savedialog.ShowDialog() == DialogResult.OK) 
                {
                    try
                    {
                        image.Save(savedialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно сохранить изображение", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvertFilter filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).ProcessImage(image, backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
                image = newImage;
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled) //если событие не отменено
            {
                pictureBox1.Image = image;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters gausfilter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(gausfilter);
        }

        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters sepia = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(sepia);
        }

        private void grayScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters grayscale = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(grayscale);
        }

        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters bright = new BrightFilter();
            backgroundWorker1.RunWorkerAsync(bright);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters sharp = new SharpFilter();
            backgroundWorker1.RunWorkerAsync(sharp);
        }

        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters stamp = new StampFilter();
            backgroundWorker1.RunWorkerAsync(stamp);
        }

        private void фильтрСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters sobel = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(sobel);
        }

        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters mirror = new MirrorFilter();
            backgroundWorker1.RunWorkerAsync(mirror);
        }

        private void волны1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters wave1 = new WaveFilter1();
            backgroundWorker1.RunWorkerAsync(wave1);
        }

        private void волны2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters wave2 = new WaveFilter2();
            backgroundWorker1.RunWorkerAsync(wave2);
        }

        private void поворотToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters turn = new TurnFilter();
            backgroundWorker1.RunWorkerAsync(turn);
        }

        private void сильнаяРезкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters sharp2 = new SharpFilter2();
            backgroundWorker1.RunWorkerAsync(sharp2);
        }

        private void размытостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters motionblur = new MotionBlur();
            backgroundWorker1.RunWorkerAsync(motionblur);
        }

        private void переносToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters transfer = new TransferFilter();
            backgroundWorker1.RunWorkerAsync(transfer);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GrayWorld filter = new GrayWorld();
            Bitmap resultImage = filter.ProcessImage(image);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }

        private void линейнаяГистограммаToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            GistFilter gist = new GistFilter();
            Bitmap resultImage = gist.ProcessImage(image);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }

        private void медианныйФильтрToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MedianFilter median = new MedianFilter();
            Bitmap resultImage = median.ProcessImage(image);
            pictureBox1.Image = resultImage;
            pictureBox1.Refresh();
        }

        private void dilationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters dilation = new DilationFilter();
            backgroundWorker1.RunWorkerAsync(dilation);
        }

        private void erosionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters erosion = new ErosionFilter();
            backgroundWorker1.RunWorkerAsync(erosion);
        }

        private void openingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters opening = new OpeningFilter();
            backgroundWorker1.RunWorkerAsync(opening);
        }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters closing = new ClosingFilter();
            backgroundWorker1.RunWorkerAsync(closing);
        }

        private void blackHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters blackhat = new BlackHat();
            backgroundWorker1.RunWorkerAsync(blackhat);
        }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters tophat = new TopHat();
            backgroundWorker1.RunWorkerAsync(tophat);
        }

        private void фильтрЩарраToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters scharra = new ScharraFilter();
            backgroundWorker1.RunWorkerAsync(scharra);
        }

        private void фильтрПрюиттаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters pruitta = new PruittaFilter();
            backgroundWorker1.RunWorkerAsync(pruitta);
        }

       

    }    
}
