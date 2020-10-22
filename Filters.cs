using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WindowsFormsApp2;
using System.Drawing.Imaging;
using System.Collections;

namespace WindowsFormsApp2
{
    abstract class Filters
    {
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
       
        public virtual Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            //List<Bitmap> list = new List<Bitmap>() ;
            for (int i = 0; i < sourceImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                {
                    return null;
                }
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            //list.Add(new Bitmap(resultImage));
            return resultImage;
        }



        public int Clamp(int value, int min, int max)
        {
            if (value > max)
            {
                return max;
            }
            if (value < min)
            {
                return min;
            }
            return value;
        }

        public class InvertFilter : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color sourceColor = sourceImage.GetPixel(x, y);
                Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
                return resultColor;
            }
        }

        public class MatrixFilter : Filters
        {
            protected float[,] kernel = null;
            protected MatrixFilter() { }
            protected MatrixFilter(Bitmap mp) { }
            public MatrixFilter(float[,] kernel)
            {
                this.kernel = kernel;
            }

            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {

                int radiusX = kernel.GetLength(0) / 2;
                int radiusY = kernel.GetLength(1) / 2;
                float resultR = 0;
                float resultG = 0;
                float resultB = 0;
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    for (int k = -radiusX; k <= radiusX; k++)
                    {
                        int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                        int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                        Color neighborColor = sourceImage.GetPixel(idX, idY);
                        resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                        resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                        resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                    }
                }
                return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
            }
        }

        public class BlurFilter : MatrixFilter
        {
            public BlurFilter()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                for (int i = 0; i < sizeX; i++)
                {
                    for (int j = 0; j < sizeY; j++)
                    {
                        kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                    }
                }
            }
        }
        public class GaussianFilter : MatrixFilter
        {
            public void createGaussianKernel(int radius, float sigma)
            {
                int size = radius * 2 + 1;
                kernel = new float[size, size];
                float norm = 0;
                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (2 * sigma * sigma)));
                        norm += kernel[i + radius, j + radius];
                    }
                }
                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        kernel[i, j] /= norm;
                    }

            }
            public GaussianFilter()
            {

                createGaussianKernel(7, 2);
            }
        }

        public class GrayScaleFilter : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color sourceColor = sourceImage.GetPixel(x, y);
                float R = sourceColor.R;
                float G = sourceColor.G;
                float B = sourceColor.B;
                float intensity = 0.299f * R + 0.587f * G + 0.114f * B;
                Color resultColor = Color.FromArgb((int)intensity, (int)intensity, (int)intensity);
                return resultColor;

            }
        }

        public class SepiaFilter : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color sourceColor = sourceImage.GetPixel(x, y);
                float R = sourceColor.R;
                float G = sourceColor.G;
                float B = sourceColor.B;

                float intensity = 0.299f * R + 0.587f * G + 0.114f * B;
                R = intensity + 2 * 15;
                G = intensity + 0.5f * 15;
                B = intensity - 1 * 15;
                return Color.FromArgb(Clamp((int)R, 0, 255), Clamp((int)G, 0, 255), Clamp((int)B, 0, 255));

            }
        }
        public class BrightFilter : Filters

        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color sourceColor = sourceImage.GetPixel(x, y);
                float c = 25.0f;
                float R = sourceColor.R + c;
                float G = sourceColor.G + c;
                float B = sourceColor.B + c;
                return Color.FromArgb(Clamp((int)R, 0, 255), Clamp((int)G, 0, 255), Clamp((int)B, 0, 255));
            }
        }
        
        public class SharpFilter : MatrixFilter
        {
            public SharpFilter()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = 0;
                kernel[0, 1] = -1;
                kernel[0, 2] = 0;

                kernel[1, 0] = -1;
                kernel[1, 1] = 5;
                kernel[1, 2] = -1;

                kernel[2, 0] = 0;
                kernel[2, 1] = -1;
                kernel[2, 2] = 0;

            }
        }

        public class StampFilter : MatrixFilter
        {
            protected override Color calculateNewPixelColor(Bitmap resultImage, int x, int y)
            {
                int radiusX = kernel.GetLength(0) / 2;
                int radiusY = kernel.GetLength(1) / 2;
                float resultR = 0;
                float resultG = 0;
                float resultB = 0;
                for (int l = -radiusY; l <= radiusY; l++)
                {
                    for (int k = -radiusX; k <= radiusX; k++)
                    {
                        int idX = Clamp(x + k, 0, resultImage.Width - 1);
                        int idY = Clamp(y + l, 0, resultImage.Height - 1);
                        Color neighborColor = resultImage.GetPixel(idX, idY);
                        float intensity = 0.299f * neighborColor.R + 0.587f * neighborColor.G + 0.114f * neighborColor.B;
                        resultR += intensity * kernel[k + radiusX, l + radiusY];
                        resultG += intensity * kernel[k + radiusX, l + radiusY];
                        resultB += intensity * kernel[k + radiusX, l + radiusY];
                    }
                }
                return Color.FromArgb(Clamp(((int)resultR + 255) / 2, 0, 255), Clamp(((int)resultG + 255) / 2, 0, 255), Clamp(((int)resultB + 255) / 2, 0, 255));
            }


            public StampFilter()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = 0;
                kernel[0, 1] = 1;
                kernel[0, 2] = 0;

                kernel[1, 0] = 1;
                kernel[1, 1] = 0;
                kernel[1, 2] = -1;

                kernel[2, 0] = 0;
                kernel[2, 1] = -1;
                kernel[2, 2] = 0;
            }

        }
        public class SobelFilterX : MatrixFilter
        {
            public SobelFilterX()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = -1;
                kernel[0, 1] = 0;
                kernel[0, 2] = 1;

                kernel[1, 0] = -2;
                kernel[1, 1] = 0;
                kernel[1, 2] = 2;

                kernel[2, 0] = -1;
                kernel[2, 1] = 0;
                kernel[2, 2] = 1;
            }
        }

        public class SobelFilterY : MatrixFilter
        {
            public SobelFilterY()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = -1;
                kernel[0, 1] = -2;
                kernel[0, 2] = -1;

                kernel[1, 0] = 0;
                kernel[1, 1] = 0;
                kernel[1, 2] = 0;

                kernel[2, 0] = 1;
                kernel[2, 1] = 2;
                kernel[2, 2] = 1;
            }
        }
        public class SobelFilter : MatrixFilter
        {
            Filters FilterX;
            Filters FilterY;
            public SobelFilter()
            {
                FilterX = new SobelFilterX();
                FilterY = new SobelFilterY();
            }
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
                Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
                float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R + dY.R);
                float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G + dY.G);
                float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B + dY.B);
                return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
            }
        }

        public class TurnFilter : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int x0 = sourceImage.Width / 2;
                int y0 = sourceImage.Height / 2;
                double M = Math.PI / 3;
                
                int newX = Clamp((int)((x - x0) * Math.Cos(M) - (y - y0) * Math.Sin(M) + x0), 0, sourceImage.Width - 1);
                int newY = Clamp((int)((x - x0) * Math.Sin(M) + (y - y0) * Math.Cos(M) + y0), 0, sourceImage.Height - 1);
                int xx = (int)(x - x0);
                int yy = (int)(y - y0);
                Bitmap result = new Bitmap(xx, yy);
                
                Graphics g = Graphics.FromImage(result);
                g.Clear(Color.White);
                return result.GetPixel(newX, newY);
            }
        }
        public class TransferFilter : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int newX = Clamp((int)(x + 50), 0, sourceImage.Width - 1);
                int newY = Clamp((int)(y), 0, sourceImage.Height - 1);
                return sourceImage.GetPixel(newX, newY);
            }
        }


        public class WaveFilter1 : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int newX = Clamp((int)(x + (20 * Math.Sin(2 * Math.PI * y / 60))), 0, sourceImage.Width - 1);
                int newY = Clamp((int)(y), 0, sourceImage.Height - 1);
                return sourceImage.GetPixel(newX, newY);
            }
        }

        public class WaveFilter2 : Filters
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int newX = Clamp((int)(x + (20 * Math.Sin(2 * Math.PI * x / 30))), 0, sourceImage.Width - 1);
                int newY = Clamp((int)(y), 0, sourceImage.Height - 1);
                return sourceImage.GetPixel(newX, newY);
            }
        }

        public class MirrorFilter : Filters
        {
            Random rand = new Random();
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                int newX = Clamp((int)(x + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Width - 1);
                int newY = Clamp((int)(y + (rand.NextDouble() - 0.5) * 10), 0, sourceImage.Height - 1);
                return sourceImage.GetPixel(newX, newY);
            }
        }

        public class MotionBlur : MatrixFilter
        {
            public MotionBlur()
            {
                int size = 11;
                kernel = new float[size, size];
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (i != j)
                            kernel[i, j] = 0;
                        else
                            kernel[i, j] = 1.0f / size; 
                    }
                
                }
            }
        }
        public class SharpFilter2 : MatrixFilter
        {
            public SharpFilter2()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = -1;
                kernel[0, 1] = -1;
                kernel[0, 2] = -1;

                kernel[1, 0] = -1;
                kernel[1, 1] = 9;
                kernel[1, 2] = -1;

                kernel[2, 0] = -1;
                kernel[2, 1] = -1;
                kernel[2, 2] = -1;

            }
        }
        public class ScharraFilterX : MatrixFilter
        {
            public ScharraFilterX()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = 3;
                kernel[0, 1] = 0;
                kernel[0, 2] = -3;

                kernel[1, 0] = 10;
                kernel[1, 1] = 0;
                kernel[1, 2] = -10;

                kernel[2, 0] = 3;
                kernel[2, 1] = 0;
                kernel[2, 2] = -3;
            }
        }

        public class ScharraFilterY : MatrixFilter
        {
            public ScharraFilterY()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = 3;
                kernel[0, 1] = 0;
                kernel[0, 2] = 3;

                kernel[1, 0] = 0;
                kernel[1, 1] = 0;
                kernel[1, 2] = 0;

                kernel[2, 0] = -3;
                kernel[2, 1] = -10;
                kernel[2, 2] = -3;
            }
        }
        public class ScharraFilter : MatrixFilter
        {
            Filters FilterX;
            Filters FilterY;
            public ScharraFilter()
            {
                FilterX = new ScharraFilterX();
                FilterY = new ScharraFilterY();
            }
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
                Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
                float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R + dY.R);
                float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G + dY.G);
                float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B + dY.B);
                return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
            }
        }

        public class PruittaFilterX : MatrixFilter
        {
            public PruittaFilterX()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = -1;
                kernel[0, 1] = 0;
                kernel[0, 2] = 1;

                kernel[1, 0] = -1;
                kernel[1, 1] = 0;
                kernel[1, 2] = 1;

                kernel[2, 0] = -1;
                kernel[2, 1] = 0;
                kernel[2, 2] = 1;
            }
        }

        public class PruittaFilterY : MatrixFilter
        {
            public PruittaFilterY()
            {
                int sizeX = 3;
                int sizeY = 3;
                kernel = new float[sizeX, sizeY];
                kernel[0, 0] = -1;
                kernel[0, 1] = -1;
                kernel[0, 2] = -1;

                kernel[1, 0] = 0;
                kernel[1, 1] = 0;
                kernel[1, 2] = 0;

                kernel[2, 0] = 1;
                kernel[2, 1] = 1;
                kernel[2, 2] = 1;
            }
        }
        public class PruittaFilter : MatrixFilter
        {
            Filters FilterX;
            Filters FilterY;
            public PruittaFilter()
            {
                FilterX = new PruittaFilterX();
                FilterY = new PruittaFilterY();
            }
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color dX = FilterX.calculateNewPixelColor(sourceImage, x, y);
                Color dY = FilterY.calculateNewPixelColor(sourceImage, x, y);
                float resultR = (float)Math.Sqrt(dX.R * dX.R + dY.R + dY.R);
                float resultG = (float)Math.Sqrt(dX.G * dX.G + dY.G + dY.G);
                float resultB = (float)Math.Sqrt(dX.B * dX.B + dY.B + dY.B);
                return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
            }
        }
        public class GrayWorld
        {
            public int Clamp(int value, int min, int max)
            {
                if (value > max)
                {
                    return max;
                }
                if (value < min)
                {
                    return min;
                }
                return value;
            }
            public Bitmap ProcessImage(Bitmap sourceImage)
            {
                int R = 0;
                int G = 0;
                int B = 0;
                double N = sourceImage.Width * sourceImage.Height;
                Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

                for (int i = 0; i < sourceImage.Width; i++)
                {
                    for (int j = 0; j < sourceImage.Height; j++)
                    {
                        Color sourceColor = sourceImage.GetPixel(i, j);
                        R += (Int32)sourceColor.R;
                        G += (Int32)sourceColor.G;
                        B += (Int32)sourceColor.B;
                    }
                }
                double AvgR = R / N;
                double AvgG = G / N;
                double AvgB = B / N;
                double Avg = (AvgR + AvgG + AvgB) / 3;
                for (int i = 0; i < sourceImage.Width; i++)
                {
                    for (int j = 0; j < sourceImage.Height; j++)
                    {
                        Color sourceColor = sourceImage.GetPixel(i, j);
                        Color resultColor = Color.FromArgb(Clamp((Int32)(sourceColor.R * Avg / AvgR), 0, 255), Clamp((Int32)(sourceColor.G * Avg / AvgG), 0, 255), Clamp((Int32)(sourceColor.B * Avg / AvgB), 0, 255));
                        resultImage.SetPixel(i, j, resultColor);
                    }
                }
                return resultImage;
            }

        }
        public class GistFilter
        {
            public int Clamp(int value, int min, int max)
            {
                if (value > max)
                {
                    return max;
                }
                if (value < min)
                {
                    return min;
                }
                return value;
            } 
            public Bitmap ProcessImage(Bitmap sourceImage)
            {
                Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
                float min = 255;
                float max = 0;
                for (int i = 0; i < sourceImage.Width; i++)
                    for (int j = 0; j < sourceImage.Height; j++)
                    {
                        
                        float inten = (sourceImage.GetPixel(i, j).R + sourceImage.GetPixel(i, j).G + sourceImage.GetPixel(i, j).B) / 3;
                        if (inten > max)
                        {
                            max = inten;
                        }
                        if (inten < min)
                        {
                            min = inten;
                        }
                    }
                if (min == max)
                {
                    max++;
                }
                for (int i = 0; i < sourceImage.Width; i++)
                    for (int j = 0; j < sourceImage.Height; j++)
                    {
                        float inten = (sourceImage.GetPixel(i, j).R + sourceImage.GetPixel(i, j).G + sourceImage.GetPixel(i, j).B) / 3;
                        float g = (inten - min) * (255 / (max - min));
                        float resR = sourceImage.GetPixel(i, j).R / inten;
                        float resG = sourceImage.GetPixel(i, j).G / inten;
                        float resB = sourceImage.GetPixel(i, j).B / inten;
                        Color resultColor = Color.FromArgb(Clamp((int)(g * resR), 0, 255), Clamp((int)(g * resG), 0, 255), Clamp((int)(g * resB), 0, 255));
                        resultImage.SetPixel(i, j, resultColor);
                    }
                return resultImage;
            }  
        }
        public class MedianFilter
        {
            public int Clamp(int value, int min, int max)
            {
                if (value > max)
                {
                    return max;
                }
                if (value < min)
                {
                    return min;
                }
                return value;
            }
            public Bitmap ProcessImage(Bitmap sourceImage)
            {

                Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
                int[] massR = new int[3 * 3];
                int[] massG = new int[3 * 3];
                int[] massB = new int[3 * 3];

                for (int i = 1; i < sourceImage.Height - 1; i++)
                {
                    for (int j = 1; j < sourceImage.Width - 1; j++)
                    { //RedPixel
                        massR[0] = sourceImage.GetPixel(j - 1, i - 1).R;
                        massR[1] = sourceImage.GetPixel(j - 1, i).R;
                        massR[2] = sourceImage.GetPixel(j - 1, i + 1).R;
                        massR[3] = sourceImage.GetPixel(j + 1, i - 1).R;
                        massR[4] = sourceImage.GetPixel(j + 1, i + 1).R;
                        massR[5] = sourceImage.GetPixel(j + 1, i).R;
                        massR[6] = sourceImage.GetPixel(j, i - 1).R;
                        massR[7] = sourceImage.GetPixel(j, i + 1).R;
                        massR[8] = sourceImage.GetPixel(j, i).R;

                        //GreenPixel
                        massG[0] = sourceImage.GetPixel(j - 1, i - 1).G;
                        massG[1] = sourceImage.GetPixel(j - 1, i).G;
                        massG[2] = sourceImage.GetPixel(j - 1, i + 1).G;
                        massG[3] = sourceImage.GetPixel(j + 1, i - 1).G;
                        massG[4] = sourceImage.GetPixel(j + 1, i + 1).G;
                        massG[5] = sourceImage.GetPixel(j + 1, i).G;
                        massG[6] = sourceImage.GetPixel(j, i - 1).G;
                        massG[7] = sourceImage.GetPixel(j, i + 1).G;
                        massG[8] = sourceImage.GetPixel(j, i).G;

                        //BluePixel
                        massB[0] = sourceImage.GetPixel(j - 1, i - 1).B;
                        massB[1] = sourceImage.GetPixel(j - 1, i).B;
                        massB[2] = sourceImage.GetPixel(j - 1, i + 1).B;
                        massB[3] = sourceImage.GetPixel(j + 1, i - 1).B;
                        massB[4] = sourceImage.GetPixel(j + 1, i + 1).B;
                        massB[5] = sourceImage.GetPixel(j + 1, i).B;
                        massB[6] = sourceImage.GetPixel(j, i - 1).B;
                        massB[7] = sourceImage.GetPixel(j, i + 1).B;
                        massB[8] = sourceImage.GetPixel(j, i).B;

                        Array.Sort(massR);
                        Array.Sort(massG);
                        Array.Sort(massB);
                        Color resultColor = Color.FromArgb(Clamp(massR[4], 0, 255), Clamp(massG[4], 0, 255), Clamp(massB[4], 0, 255));
                        resultImage.SetPixel(j, i, resultColor);
                    }
                }
                return resultImage;
            }
        }

        public abstract class MorfologeFilter : Filters
        {
            protected int MW = 3, MH = 3; //размеры структурного множества
            protected int[,] mask = { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } };

            public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
            {
                Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);

                for (int i = MW / 2; i < sourceImage.Width - MW / 2; i++)
                {
                    worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                    if (worker.CancellationPending)
                        return null;

                    for (int j = MH / 2; j < sourceImage.Height - MH / 2; j++)
                        resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }

                return resultImage;
            }
        }

        public class DilationFilter : MorfologeFilter
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color max = Color.FromArgb(0, 0, 0);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                            max = pixel;
                    }

                return max;
            }
        }
        public class ErosionFilter : MorfologeFilter
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color min = Color.FromArgb(255, 255, 255);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                            min = pixel;
                    }

                return min;
            }
        }

        public class OpeningFilter : MorfologeFilter
        {
            public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
            {
                Bitmap erosion = new Bitmap(sourceImage.Width, sourceImage.Height);

                for (int i = MW / 2; i < erosion.Width - MW / 2; i++)
                {
                    worker.ReportProgress((int)((float)i / erosion.Width * 50));
                    if (worker.CancellationPending)
                        return null;

                    for (int j = MH / 2; j < erosion.Height - MH / 2; j++)
                        erosion.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }

                Bitmap result = new Bitmap(erosion);

                for (int i = MW / 2; i < result.Width - MW / 2; i++)
                {
                    worker.ReportProgress((int)((float)i / result.Width * 50 + 50));
                    if (worker.CancellationPending)
                        return null;

                    for (int j = MH / 2; j < result.Height - MH / 2; j++)
                        result.SetPixel(i, j, CalculateDilation(erosion, i, j));
                }

                return result;
            }

            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color min = Color.FromArgb(255, 255, 255);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                            min = pixel;
                    }

                return min;
            }

            private Color CalculateDilation(Bitmap sourceImage, int x, int y)
            {
                Color max = Color.FromArgb(0, 0, 0);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                            max = pixel;
                    }

                return max;
            }
        }
        public class ClosingFilter : MorfologeFilter
        {
            public override Bitmap ProcessImage(Bitmap sourceImage, BackgroundWorker worker)
            {
                Bitmap dilation = new Bitmap(sourceImage.Width, sourceImage.Height);

                for (int i = MW / 2; i < dilation.Width - MW / 2; i++)
                {
                    worker.ReportProgress((int)((float)i / dilation.Width * 50));
                    if (worker.CancellationPending)
                        return null;

                    for (int j = MH / 2; j < dilation.Height - MH / 2; j++)
                        dilation.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }

                Bitmap result = new Bitmap(dilation);

                for (int i = MW / 2; i < result.Width - MW / 2; i++)
                {
                    worker.ReportProgress((int)((float)i / result.Width * 50 + 50));
                    if (worker.CancellationPending)
                        return null;

                    for (int j = MH / 2; j < result.Height - MH / 2; j++)
                        result.SetPixel(i, j, CalculateErosion(dilation, i, j));
                }

                return result;
            }

            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {

                Color max = Color.FromArgb(0, 0, 0);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                            max = pixel;
                    }

                return max;
            }

            private Color CalculateErosion(Bitmap sourceImage, int x, int y)
            {
                Color min = Color.FromArgb(255, 255, 255);

                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                            min = pixel;
                    }

                return min;
            }
        }

        public class BlackHat : MorfologeFilter // расширение - исходное
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color max = Color.FromArgb(0, 0, 0);
                Color sourceColor = sourceImage.GetPixel(x, y);
                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] == 1 && pixel.R > max.R && pixel.G > max.G && pixel.B > max.B)
                            max = pixel;
                    }
                return Color.FromArgb(Clamp((int)(max.R - sourceColor.R), 0, 255), Clamp((int)(max.G - sourceColor.G), 0, 255), Clamp((int)(max.B - sourceColor.B), 0, 255));
            }

        }
        public class TopHat : MorfologeFilter //исходное - сужение
        {
            protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
            {
                Color min = Color.FromArgb(255, 255, 255);
                Color sourceColor = sourceImage.GetPixel(x, y);
                for (int j = -MH / 2; j <= MH / 2; j++)
                    for (int i = -MW / 2; i <= MW / 2; i++)
                    {
                        Color pixel = sourceImage.GetPixel(x + i, y + j);

                        if (mask[i + MW / 2, j + MH / 2] != 0 && pixel.R < min.R && pixel.G < min.G && pixel.B < min.B)
                            min = pixel;
                       
                    }
                return Color.FromArgb(Clamp((int)(sourceColor.R - min.R), 0, 255), Clamp((int)(sourceColor.G - min.G), 0, 255), Clamp((int)(sourceColor.B - min.B), 0, 255));
            }

        }
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
