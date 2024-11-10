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
        private static TextBox textBox1;
        private static double[,] kernel1;
        private static int k;

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void MyProc2(byte* ptr, byte valueToAdd);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void calculateFilterCPP(byte* ptr, int row, int width, int height, int k, double[,] filter);

        public MainForm()
        {
            // Initialize components
            textBox1 = new TextBox()
            {
                Dock = DockStyle.Bottom
            };
            pictureBox1 = new PictureBox
            {
                Width = 600,
                Height = 600,
                SizeMode = PictureBoxSizeMode.Zoom,
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

            Controls.Add(textBox1);
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
        public static double[,] GenerateGaussianKernel(double sigma)
        {
            if (int.TryParse(textBox1.Text, out int size))
            {
                MessageBox.Show($"Wpisana liczba: {size}", "Informacja");
                k = (size - 1) / 2;
            }
            else
            {
                MessageBox.Show("Wpisano nieprawidłową liczbę!", "Błąd");
            }

            double[,] kernel = new double[size, size];
            int center = size / 2;
            double sum = 0.0;

            // Oblicz wartości dla maski
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int x = i - center;
                    int y = j - center;
                    kernel[i, j] = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    sum += kernel[i, j];
                }
            }

            // Normalizacja
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= sum;
                }
            }

            return kernel;
        }

        private static unsafe Bitmap ProcessBitmap(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone();
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            byte* ptr = (byte*)bmpData.Scan0;

            // Przechowujemy szerokość i wysokość w zmiennych lokalnych
            int width = bmp.Width;
            int height = bmp.Height;

            // Tworzymy tablicę wątków

            Stopwatch sw = new Stopwatch();
            Thread[] threads = new Thread[height];
            sw.Start();
            for (int i = 0; i < height; i++)
            {
                int currentRow = i; // Kopiujemy indeks, aby uniknąć zamknięć
                threads[currentRow] = new Thread(() =>
                {
                    calculateFilterCPP(ptr, currentRow, width, height, k, kernel1);

                });
                threads[currentRow].Start();
            }

            foreach (Thread t in threads)
            {
                t.Join(); // Oczekiwanie na zakończenie wątków
            }
            sw.Stop();
            bmap.UnlockBits(bmpData);
            MessageBox.Show($"Czas wykonania: {sw.ElapsedMilliseconds} ms");
            return bmap;
        }


        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private bool rightImageFormat(Bitmap bitmap)
        {
            if (bitmap.PixelFormat != PixelFormat.Format24bppRgb)
            {
                MessageBox.Show("Obsługiwane są tylko obrazy 24-bitowe RGB.");
                return false;
            }
            else
            {
                MessageBox.Show("Poprawny obraz");
                return true;
            }
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
                    if (rightImageFormat(bitmap))
                    {
                        kernel1 = GenerateGaussianKernel(100);
                        pictureBox1.Image = bitmap;
                        bitmap = ProcessBitmap(bitmap);
                        pictureBox2.Image = bitmap;
                    }
                }
            }
        }
    }
}