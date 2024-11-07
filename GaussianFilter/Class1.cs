using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GaussianFilter
{
    public class MainForm : Form
    {
        private PictureBox originalPictureBox;
        private PictureBox processedPictureBox;
        private Bitmap bitmap;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private Button button1;
        private byte[] bitmapBytes;

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void MyProc2(byte* ptr, byte valueToAdd);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void calculateFilterCPP(byte* ptr, int imWidth);

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

            button1 = new Button
            {
                Text = "Wczytaj obraz",
                Dock = DockStyle.Top
            };
            button1.Click += button1_Click;

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

        private static unsafe void ProcessBitmap(byte[] bitmapBytes, int imWidth)
        {
            // Definiujemy filtr Gaussa (3x3)
            short[] filter = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };

            fixed (byte* basePtr = bitmapBytes)
            {
                int length = bitmapBytes.Length;
                int chunkSize = imWidth * 3; // zakładamy RGB (3 bajty na piksel)
                int threadCount = 4; // liczba wątków, można dostosować według potrzeby
                Thread[] threads = new Thread[threadCount];

                for (int i = 0; i < threadCount; i++)
                {
                    int start = i * (length / threadCount);
                    int end = (i + 1) * (length / threadCount);

                    // Tworzymy lokalną kopię wskaźnika do obrazu
                    byte* threadPtr = basePtr;

                    // Tworzymy kopię filtra dla każdego wątku
                    short[] filterCopy = (short[])filter.Clone();

                    threads[i] = new Thread(() =>
                    {
                        fixed (short* filterPtr = filterCopy) // używamy `fixed` wewnątrz lambdy dla każdej kopii filtra
                        {
                            for (int j = start; j < end; j += chunkSize)
                            {
                                byte* pixelPtr = threadPtr + j;

                                // Wywołujemy calculateFilterCPP z wskaźnikiem piksela i filtrem
                                calculateFilterCPP(pixelPtr, imWidth);
                            }
                        }
                    });
                    threads[i].Start();
                }

                // Czekamy na zakończenie wszystkich wątków
                foreach (var t in threads)
                {
                    t.Join();
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

        private void InitializeComponent()
        {
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 36);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(283, 248);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(445, 36);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(267, 248);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(335, 142);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(740, 532);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.Name = "MainForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

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
                }
            }
        }
    }
}