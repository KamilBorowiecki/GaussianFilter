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

        private static unsafe void GaussianBlurRow(byte* ptr, int width, int height, int bytesPerPixel, int stride, int row)
        {
            // Jądro filtra Gaussa 9x9
            double[,] kernel = {
            { 1 / 273.0,  1 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  1 / 273.0,  1 / 273.0, 1 / 273.0 },
            { 1 / 273.0,  2 / 273.0,  2 / 273.0,  4 / 273.0,  4 / 273.0,  2 / 273.0,  2 / 273.0,  1 / 273.0, 1 / 273.0 },
            { 2 / 273.0,  2 / 273.0,  4 / 273.0,  8 / 273.0,  8 / 273.0,  4 / 273.0,  2 / 273.0,  2 / 273.0, 2 / 273.0 },
            { 2 / 273.0,  4 / 273.0,  8 / 273.0, 16 / 273.0, 16 / 273.0,  8 / 273.0,  4 / 273.0,  2 / 273.0, 2 / 273.0 },
            { 2 / 273.0,  4 / 273.0,  8 / 273.0, 16 / 273.0, 16 / 273.0,  8 / 273.0,  4 / 273.0,  2 / 273.0, 2 / 273.0 },
            { 2 / 273.0,  4 / 273.0,  8 / 273.0, 16 / 273.0, 16 / 273.0,  8 / 273.0,  4 / 273.0,  2 / 273.0, 2 / 273.0 },
            { 1 / 273.0,  2 / 273.0,  2 / 273.0,  4 / 273.0,  4 / 273.0,  2 / 273.0,  2 / 273.0,  1 / 273.0, 1 / 273.0 },
            { 1 / 273.0,  1 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  1 / 273.0,  1 / 273.0, 1 / 273.0 },
            { 1 / 273.0,  1 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  2 / 273.0,  1 / 273.0,  1 / 273.0, 1 / 273.0 }};

            // Bufor na przetworzone dane dla danego wiersza
            byte[] result = new byte[width * bytesPerPixel];

            // Przetwarzanie pikseli w wybranym wierszu (z pominięciem brzegów)
            for (int x = 4; x < width - 4; x++)
            {
                double blueSum = 0, greenSum = 0, redSum = 0;

                // Przechodzimy przez jądro 9x9
                for (int ky = -4; ky <= 4; ky++)
                {
                    for (int kx = -4; kx <= 4; kx++)
                    {
                        int neighborX = x + kx;
                        int neighborY = row + ky;

                        // Sprawdzamy, czy sąsiad mieści się w granicach obrazu
                        if (neighborY >= 0 && neighborY < height)
                        {
                            byte* neighborPixel = ptr + (neighborY * stride) + (neighborX * bytesPerPixel);

                            int neighborBlue = neighborPixel[0];
                            int neighborGreen = neighborPixel[1];
                            int neighborRed = neighborPixel[2];

                            double kernelValue = kernel[ky + 4, kx + 4];
                            blueSum += neighborBlue * kernelValue;
                            greenSum += neighborGreen * kernelValue;
                            redSum += neighborRed * kernelValue;
                        }
                    }
                }

                // Zapisanie wyników dla bieżącego piksela
                int resultIndex = x * bytesPerPixel;
                result[resultIndex] = (byte)Clamp((int)blueSum, 0, 255);
                result[resultIndex + 1] = (byte)Clamp((int)greenSum, 0, 255);
                result[resultIndex + 2] = (byte)Clamp((int)redSum, 0, 255);
            }

            // Przeniesienie wyników z powrotem do pamięci oryginalnej bitmapy
            byte* rowPtr = ptr + (row * stride);
            for (int i = 0; i < width * bytesPerPixel; i++)
            {
                rowPtr[i] = result[i];
            }
        }

        private static unsafe Bitmap ProcessBitmap(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone(); // Clone the bitmap to avoid modifying the original
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(bmap.PixelFormat) / 8;
            int stride = bmpData.Stride;
            byte* ptr = (byte*)bmpData.Scan0;

            // Przechowujemy szerokość i wysokość w zmiennych lokalnych
            int width = bmp.Width;
            int height = bmp.Height;

            // Tworzymy tablicę wątków
            Thread[] threads = new Thread[height];

            for (int i = 0; i < height; i++)
            {
                int currentRow = i; // Kopiujemy indeks, aby uniknąć zamknięć
                threads[currentRow] = new Thread(() =>
                {
                    GaussianBlurRow(ptr, width, height, bytesPerPixel, stride, currentRow);
                });
                threads[currentRow].Start();
            }

            foreach (Thread t in threads)
            {
                t.Join(); // Oczekiwanie na zakończenie wątków
            }

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