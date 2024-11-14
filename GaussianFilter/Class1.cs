using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace GaussianFilter
{
    public class MainForm : Form
    {
        private Bitmap bitmap;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button button1;
        private static TextBox textBox1;
        private static double[] filter;
        private static int k;

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void MyProc2(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern void calculateFilterCPP(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter, int k);

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
        public static double[] GenerateGaussianKernel(double sigma)
        {
            int size = int.Parse(textBox1.Text);
            k = (size - 1) / 2; // Rozmiar półmaski
            double[] gaussianFilter = new double[size * size];
            int center = size / 2;
            double sum = 0.0;

            // Oblicz wartości dla maski
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int x = i - center;
                    int y = j - center;
                    int index = i * size + j; // Konwersja indeksów 2D na 1D
                    gaussianFilter[index] = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    sum += gaussianFilter[index];
                }
            }

            // Normalizacja
            for (int i = 0; i < gaussianFilter.Length; i++)
            {
                gaussianFilter[i] /= sum;
            }

            return gaussianFilter;
        }


        private static unsafe Bitmap ProcessBitmap(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone();
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // Przechowujemy szerokość i wysokość w zmiennych lokalnych
            int width = bmp.Width;
            int height = bmp.Height;

            int bytesPerPixel = 3; // 24-bitowy BMP
            int stride = bmpData.Stride; // Uwzględniamy rzeczywisty stride bitmapy
            uint imageSize = (uint)(stride * height);

            byte[] data = new byte[imageSize];
            byte[] outData = new byte[imageSize];

            // Kopiujemy dane z bitmapy do tablicy bajtów
            Marshal.Copy(ptr, data, 0, data.Length);

            // Przetwarzanie obrazu przy użyciu filtra w C++
            Int16[] filter1 = {1,2,1,2,4,2,1,2,1};
            double[] filter2 = { 1.0 / 16, 2.0 / 16, 1.0 / 16, 2.0 / 16, 4.0 / 16, 2.0 / 16, 1.0 / 16, 2.0 / 16, 1.0 / 16 };
            //filter;
            int k1 = k;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
            {
                calculateFilterCPP(outData, data, width, i, filter1,1);
            }
            for (int iteration = 0; iteration < 3; iteration++)
            {
                for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
                {
                    calculateFilterCPP(outData, outData, width, i, filter1, 1);
                }
            }

            sw.Stop();

            // Kopiujemy dane z powrotem do bitmapy
            Marshal.Copy(outData, 0, ptr, outData.Length);

            // Odblokowujemy dane bitmapy
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
                        //filter = GenerateGaussianKernel(100);
                        pictureBox1.Image = bitmap;

                        // Przetwarzanie obrazu
                        Bitmap processedBitmap = ProcessBitmap(bitmap);
                        pictureBox2.Image = processedBitmap;
                    }
                }
            }
        }

    }
}