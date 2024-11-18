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
        private PictureBox pictureBox3;
        private Button button1;
        private static TextBox textBox1;
        private static Label leftImageTime;
        private static Label rightImageTime;
        private static Label compareResult;

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void MyProc2(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern void calculateFilterCPP(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter);

        public MainForm()
        {
            // Initialize components
            textBox1 = new TextBox()
            {
                Dock = DockStyle.Bottom
            };

            leftImageTime = new Label()
            {
                Text = "0",
                Width = 450,
                Height = 450,
                Location = new Point(510, 440),
                AutoSize = true,
            };

            rightImageTime = new Label()
            {
                Text = "0",
                Width = 450,
                Height = 150,
                Location = new Point(1010, 440),
                AutoSize = true,

            };

            compareResult = new Label()
            {
                Text = "0",
                Width = 450,
                Height = 550,
                Location = new Point(410, 710),
                AutoSize = true,
            };


            pictureBox1 = new PictureBox
            {
                Width = 450,
                Height = 450,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(10, 40)
            };

            pictureBox3 = new PictureBox
            {
                Width = 450,
                Height = 450,
                SizeMode = PictureBoxSizeMode.Zoom,
                Location = new Point(1010, 40)
            };

            pictureBox2 = new PictureBox
            {
                Width = 450,               // Ustaw szerokość na 200 pikseli
                Height = 450,              // Ustaw wysokość na 200 pikseli
                SizeMode = PictureBoxSizeMode.Zoom, // Skaluje obraz proporcjonalnie do rozmiaru ramki
                Location = new Point(510, 40)
            };

            button1 = new Button
            {
                Text = "Wczytaj obraz",
                Dock = DockStyle.Top
            };
            button1.Click += button1_Click;

            Controls.Add(leftImageTime);
            Controls.Add(rightImageTime);
            Controls.Add(compareResult);
            Controls.Add(textBox1);
            Controls.Add(pictureBox2);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox1);
            Controls.Add(button1);
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

            int stride = bmpData.Stride; // Uwzględniamy rzeczywisty stride bitmapy
            uint imageSize = (uint)(stride * height);

            byte[] data = new byte[imageSize];
            byte[] outData = new byte[imageSize];

            // Kopiujemy dane z bitmapy do tablicy bajtów
            Marshal.Copy(ptr, data, 0, data.Length);

            // Przetwarzanie obrazu przy użyciu filtra w C++
            Int16[] filter1 = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
            {
                calculateFilterCPP(outData, data, width, i, filter1);
            }
            for (int iteration = 0; iteration < 3; iteration++)
            {
                for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
                {
                    calculateFilterCPP(outData, outData, width, i, filter1);
                }
            }

            sw.Stop();

            // Kopiujemy dane z powrotem do bitmapy
            Marshal.Copy(outData, 0, ptr, outData.Length);

            // Odblokowujemy dane bitmapy
            bmap.UnlockBits(bmpData);

            leftImageTime.Text = $" {sw.ElapsedMilliseconds} ms";
            return bmap;
        }

        private static unsafe Bitmap ProcessBitmap2(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone();
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            // Przechowujemy szerokość i wysokość w zmiennych lokalnych
            int width = bmp.Width;
            int height = bmp.Height;

            int stride = bmpData.Stride; // Uwzględniamy rzeczywisty stride bitmapy
            uint imageSize = (uint)(stride * height);

            byte[] data = new byte[imageSize];
            byte[] outData = new byte[imageSize];

            // Kopiujemy dane z bitmapy do tablicy bajtów
            Marshal.Copy(ptr, data, 0, data.Length);

            // Przetwarzanie obrazu przy użyciu filtra w C++
            Int16[] filter1 = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
            {
                MyProc2(outData, data, width, i, filter1);
            }
            for (int iteration = 0; iteration < 3; iteration++)
            {
                for (int i = width * 3 + 1; i < (imageSize - width * 3); i++)
                {
                    MyProc2(outData, outData, width, i, filter1);
                }
            }

            sw.Stop();


            // Kopiujemy dane z powrotem do bitmapy
            Marshal.Copy(outData, 0, ptr, outData.Length);

            // Odblokowujemy dane bitmapy
            bmap.UnlockBits(bmpData);

            rightImageTime.Text = $" {sw.ElapsedMilliseconds} ms";
            return bmap;
        }

        void CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        compareResult.Text = "Obrazy sa rozne";
                        //Console.WriteLine($"X:{x} Y:{y}: zły -  1:{bmp1.GetPixel(x, y)} 2:{bmp2.GetPixel(x, y)} ");
                    }
                }
            }
            compareResult.Text = "Obrazy sa identyczne";
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
                        pictureBox1.Image = bitmap;

                        // Przetwarzanie obrazu
                        Bitmap processedBitmap = ProcessBitmap(bitmap);
                        pictureBox2.Image = processedBitmap;

                        Bitmap processedBitmap2 = ProcessBitmap2(bitmap);
                        pictureBox3.Image = processedBitmap2;

                        CompareBitmaps(processedBitmap, processedBitmap2);
                    }
                }
            }
        }

    }
}