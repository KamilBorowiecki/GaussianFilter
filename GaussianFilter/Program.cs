using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace GaussianFilter
{
    internal class Program
    {
        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll")]
        public static extern unsafe void MyProc2(byte* a, byte b);

        static unsafe void ProcessBitmap(byte* inputPtr, byte* outputPtr)
        {
            for (int i = 0; i < 4; i++)
            {   
                byte* value = inputPtr + i; 
                if(i!=3)
                    MyProc2(value, 19);      
                *(outputPtr + i) = *value; 
            }
        }

        static unsafe void PointerOnBitmapLine(byte[] bitmapSource, byte[] bitmapOutput, int start)
        {
            fixed (byte* inputPtr = bitmapSource)   
            fixed (byte* outputPtr = bitmapOutput)
            {
                ProcessBitmap(inputPtr + start, outputPtr + start);
            }
               
        }

        static unsafe void Main(string[] args)
        {
            String line;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader("C:\\Users\\Kamil\\Desktop\\Projekt_JA_filtrGaussa\\GaussianFilter\\bitmapa.txt");
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    //write the line to console window
                    Console.WriteLine(line);
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the file
                sr.Close();
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }



            //byte[] bitmapSource = new byte[]
            //{
            //    1, 2, 3, 0,  
            //    4, 5, 6, 0,  
            //    7, 8, 9, 0   
            //};

            //byte[] bitmapOutput = new byte[12]; 

            //Thread[] threads = new Thread[3];
            //int threadsCount = bitmapSource.Length/4;

            //{
            //    for (int i = 0; i < threadsCount; i++)
            //    {
            //        int start = i * 4; 

            //        threads[i] = new Thread(() => PointerOnBitmapLine(bitmapSource, bitmapOutput, start));
            //        threads[i].Start();
            //    }

            //    foreach (Thread t in threads)
            //    {
            //        t.Join();
            //    }
            //}

            //for (int i = 0; i < bitmapOutput.Length; i++)
            //{
            //    Console.Write(bitmapOutput[i] + " ");
            //    if ((i + 1) % 4 == 0) Console.WriteLine(); 
            //}
        }
    }
}
