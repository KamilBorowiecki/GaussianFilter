using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GaussianFilter
{
    public class MainForm : Form
    {
        private Bitmap bitmap;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button button1;
        private byte[] bitmapBytes;

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void MyProc2(byte* ptr, byte valueToAdd);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void calculateFilterCPP(byte* ptr, int width, int height);

        public MainForm()
        {
            // Initialize components
            pictureBox1 = new PictureBox
            {
                Width = 600,               // Ustaw szerokość na 200 pikseli
                Height = 600,              // Ustaw wysokość na 200 pikseli
                SizeMode = PictureBoxSizeMode.Zoom, // Skaluje obraz proporcjonalnie do rozmiaru ramki
                Location = new Point(10, 40)
            };

            pictureBox2 = new PictureBox
            {
                Width = 600,               // Ustaw szerokość na 200 pikseli
                Height = 600,              // Ustaw wysokość na 200 pikseli
                SizeMode = PictureBoxSizeMode.Zoom, // Skaluje obraz proporcjonalnie do rozmiaru ramki
                Location = new Point(710, 40)
            };

            button1 = new Button
            {
                Text = "Wczytaj obraz",
                Dock = DockStyle.Top
            };
            button1.Click += button1_Click;

            Controls.Add(pictureBox2);
            Controls.Add(pictureBox1);
            Controls.Add(button1);
        }


        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            int bytesCount = bitmap.Width * bitmap.Height * 3;
            byte[] byteArray = new byte[bytesCount];
            int index = 0;

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixel = bitmap.GetPixel(x, y);
                    byteArray[index++] = pixel.R;
                    byteArray[index++] = pixel.G;
                    byteArray[index++] = pixel.B;
                }
            }
            return byteArray;
        }

        private static Bitmap ByteArrayToBitmap(byte[] byteArray, int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            int index = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte r = byteArray[index++];
                    byte g = byteArray[index++];
                    byte b = byteArray[index++];
                    bitmap.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return bitmap;
        }
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        private static unsafe void ApplyGaussianFilter(byte* ptr, int width, int height)
        {
            // Define a 3x3 Gaussian kernel
            double[,] kernel = {
        { 1 / 16.0, 1 / 8.0, 1 / 16.0 },
        { 1 / 8.0, 1 / 4.0, 1 / 8.0 },
        { 1 / 16.0, 1 / 8.0, 1 / 16.0 }
    };

            int kernelSize = 3;
            int kernelOffset = kernelSize / 2;

            // Create a temporary array to store the filtered pixel values
            byte[] result = new byte[width * height * 4];

            for (int y = kernelOffset; y < height - kernelOffset; y++)
            {
                for (int x = kernelOffset; x < width - kernelOffset; x++)
                {
                    double rSum = 0, gSum = 0, bSum = 0;

                    // Convolution operation
                    for (int ky = -kernelOffset; ky <= kernelOffset; ky++)
                    {
                        for (int kx = -kernelOffset; kx <= kernelOffset; kx++)
                        {
                            int pixelIndex = ((y + ky) * width + (x + kx)) * 4;

                            // Access each color channel
                            byte b = ptr[pixelIndex];
                            byte g = ptr[pixelIndex + 1];
                            byte r = ptr[pixelIndex + 2];

                            double kernelValue = kernel[ky + kernelOffset, kx + kernelOffset];
                            rSum += r * kernelValue;
                            gSum += g * kernelValue;
                            bSum += b * kernelValue;
                        }
                    }

                    // Set the new pixel value in the result array
                    int resultIndex = (y * width + x) * 4;
                    result[resultIndex] = (byte)Clamp((int)bSum, 0, 255);       // Blue channel
                    result[resultIndex + 1] = (byte)Clamp((int)gSum, 0, 255);   // Green channel
                    result[resultIndex + 2] = (byte)Clamp((int)rSum, 0, 255);   // Red channel
                }
            }

            // Copy the result back to the original image data
            for (int i = 0; i < width * height * 4; i++)
            {
                ptr[i] = result[i];
            }
        }

        private static unsafe Bitmap ProcessBitmap(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone(); // Clone the bitmap to avoid modifying the original
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;
            int heightInPixels = bmpData.Height;
            int widthInBytes = bmpData.Width * bytesPerPixel;

            byte* ptr = (byte*)bmpData.Scan0;

            Parallel.For(0, heightInPixels, y =>
            {
                byte* currentLine = ptr + (y * bmpData.Stride); 
                for (int i = 0; i < widthInBytes; i += bytesPerPixel)
                {
                    int oldBlue = currentLine[i];
                    int oldGreen = currentLine[i + 1];
                    int oldRed = currentLine[i + 2];

                    currentLine[i] = 0;             
                    currentLine[i + 1] = (byte)oldGreen; 
                    currentLine[i + 2] = (byte)oldRed;   
                }
            });

            bmap.UnlockBits(bmpData);
            return bmap;
        }


        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Pliki obrazów|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Wczytywanie obrazu do PictureBox
                    bitmap = new Bitmap(openFileDialog.FileName);
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

                    pictureBox1.Image = bitmap;
                    bitmap = ProcessBitmap(bitmap);
                    pictureBox2.Image = bitmap;
                }
            }
        }
    }
}