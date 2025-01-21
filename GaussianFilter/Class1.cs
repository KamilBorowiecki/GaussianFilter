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
        private static Label leftImageTime;
        private static Label rightImageTime;
        private static Label compareResult;
        private static Label numberInputLabel;
        private static TextBox numberInput;

        [DllImport("Asm.dll")]
        public static extern unsafe void MyProc2(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter);

        [DllImport("Cdll.dll")]
        public static extern void calculateFilterCPP(byte[] outData, byte[] data, int imWidth, int i, Int16[] filter);

        public MainForm()
        {

            leftImageTime = new Label()
            {
                Width = 450,
                Height = 450,
                Location = new Point(510, 440),
                AutoSize = true,
            };

            rightImageTime = new Label()
            {
                Width = 450,
                Height = 150,
                Location = new Point(1010, 440),
                AutoSize = true,

            };

            compareResult = new Label()
            {
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
                Width = 450,             
                Height = 450,             
                SizeMode = PictureBoxSizeMode.Zoom, 
                Location = new Point(510, 40)
            };

            button1 = new Button
            {
                Text = "Wczytaj obraz",
                Width = 450,               
                Height = 30,
                Location = new Point(510, 0)
            };

            numberInputLabel = new Label
            {
                Text = "Wpisz liczbę wątków:",
                Location = new Point(10, 510),
                AutoSize = true
            };

            numberInput = new TextBox
            {
                Width = 200,
                Location = new Point(150, 510)
            };
            button1.Click += button1_Click;

            Controls.Add(leftImageTime);
            Controls.Add(rightImageTime);
            Controls.Add(compareResult);

            Controls.Add(pictureBox2);
            Controls.Add(pictureBox3);
            Controls.Add(pictureBox1);
            Controls.Add(button1);
            Controls.Add(numberInputLabel);
            Controls.Add(numberInput);
        }

        private static unsafe Bitmap ProcessBitmap(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone();
            BitmapData bmpData = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            IntPtr ptr = bmpData.Scan0;


            int width = bmp.Width;
            int height = bmp.Height;

            int stride = bmpData.Stride; 
            uint imageSize = (uint)(stride * height);

            byte[] data = new byte[imageSize];
            byte[] outData = new byte[imageSize];


            Marshal.Copy(ptr, data, 0, data.Length);

            Int16[] filter1 = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            long pictureSize = imageSize - width * 3;
            int threadCount = Environment.ProcessorCount;
            if (int.TryParse(numberInput.Text, out int number))
            {
                threadCount = number;
            }
            Stopwatch sw = new Stopwatch();
            Thread[] threads = new Thread[threadCount];


            int chunkSize = (int)(pictureSize / threadCount);
            sw.Start();

            for (int t = 0; t < threadCount; t++)
            {
                int start = width * 3 + 1 + t * chunkSize;
                int end = (t == threadCount - 1) ? (int)pictureSize : start + chunkSize;

                threads[t] = new Thread(() =>
                {

                    for (int i = start; i < end; i++)
                    {
                        
                        calculateFilterCPP(outData, data, width, i, filter1);
                        
                    }
                });

                threads[t].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            sw.Stop();

            Marshal.Copy(outData, 0, ptr, outData.Length);


            bmap.UnlockBits(bmpData);

            leftImageTime.Text = $"Czas wykonania c++: {sw.ElapsedMilliseconds} ms";
            return bmap;
        }

        private static unsafe Bitmap ProcessBitmap2(Bitmap bmp)
        {
            Bitmap bmap = (Bitmap)bmp.Clone();
            BitmapData bmpData2 = bmap.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                               ImageLockMode.ReadWrite, bmap.PixelFormat);

            IntPtr ptr = bmpData2.Scan0;


            int width = bmp.Width;
            int height = bmp.Height;

            int stride = bmpData2.Stride; 
            uint imageSize = (uint)(stride * height);

            byte[] data = new byte[imageSize];
            byte[] outData = new byte[imageSize];


            Marshal.Copy(ptr, data, 0, data.Length);

            Int16[] filter1 = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
            long pictureSize = imageSize - width * 3;
            int threadCount = Environment.ProcessorCount;
            if(int.TryParse(numberInput.Text, out int number))
            {
                threadCount = number;
            }
            Stopwatch sw = new Stopwatch();
            Thread[] threads = new Thread[threadCount];

            int chunkSize = (int)(pictureSize / threadCount);
            sw.Start();

            for (int t = 0; t < threadCount; t++)
            {
                int start = width * 3 + 1 + t * chunkSize;
                int end = (t == threadCount - 1) ? (int)pictureSize : start + chunkSize;

                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        if (i < (int)pictureSize-31)
                        {
                            MyProc2(outData, data, width, i, filter1);
                        }
                            
                        
                            
                    }
                });

                threads[t].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }
            sw.Stop();
            Marshal.Copy(outData, 0, ptr, outData.Length);

            bmap.UnlockBits(bmpData2);

            rightImageTime.Text = $"Czas wykonania asemblera: {sw.ElapsedMilliseconds} ms";
            return bmap;
        }

        void CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            compareResult.Text = "Obrazy sa identyczne";
            Console.WriteLine(bmp1.Height);
            Console.WriteLine(bmp1.Width);
            for (int y = 0; y < bmp1.Height; y++)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {

                    if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                    {
                        compareResult.Text = "Obrazy sa rozne";
                        Console.WriteLine($"X:{x} Y:{y}: zły -  1:{bmp1.GetPixel(x, y)} 2:{bmp2.GetPixel(x, y)} ");
                    }
                }
            }

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
                    bitmap = new Bitmap(openFileDialog.FileName);
                    if (rightImageFormat(bitmap))
                    {
                        pictureBox1.Image = bitmap;

                        Bitmap processedBitmap = ProcessBitmap(bitmap);
                        string filePath = @"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\1.png"; // Podaj ścieżkę do pliku
                        string filePath2 = @"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\2.png";
                        processedBitmap.Save(filePath, ImageFormat.Png);
                        pictureBox2.Image = processedBitmap;


                        Bitmap processedBitmap2 = ProcessBitmap2(bitmap);
                        processedBitmap2.Save(filePath2, ImageFormat.Png);
                        pictureBox3.Image = processedBitmap2;


                        CompareBitmaps(processedBitmap, processedBitmap2);
                    }
                }
            }
        }

    }
}