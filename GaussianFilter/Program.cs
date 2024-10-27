using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GaussianFilter
{
    internal class Program
    {
        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll")]
        public static extern void MyProc2(ref byte a, byte b);


        // Metoda przetwarzania dla wątków
        static unsafe void ProcessBitmap(byte* inputPtr, byte* outputPtr)
        {
            // Przetwarzamy odpowiedni fragment danych za pomocą wskaźników
            for (int i = 0; i < 4; i++)
            {   
                byte value = *(inputPtr + i); // Pobierz wartość źródłową przez wskaźnik
                if(i!=3)
                    MyProc2(ref value, 11);      // Przekaż przez ref do MyProc2
                *(outputPtr + i) = value; // Zapisz wynik przez wskaźnik
            }
        }

        static unsafe void PointerOnBitmapLine(byte[] bitmapSource, byte[] bitmapOutput, int start)
        {
            fixed (byte* inputPtr = bitmapSource)   // Przymocowanie wskaźnika do bitmapSource
            fixed (byte* outputPtr = bitmapOutput)
            {
                ProcessBitmap(inputPtr + start, outputPtr + start);
            }
               
        }

        static unsafe void Main(string[] args)
        {
            // Dane źródłowe (bitmapa 3x3 wyrównana do 4 bajtów na linię)
            byte[] bitmapSource = new byte[]
            {
            1, 2, 3, 0,  // Pierwsza linia (3 piksele + 1 bajt wyrównania)
            4, 5, 6, 0,  // Druga linia (3 piksele + 1 bajt wyrównania)
            7, 8, 9, 0   // Trzecia linia (3 piksele + 1 bajt wyrównania)
            };

            // Dane wyjściowe
            byte[] bitmapOutput = new byte[12]; // Też wyrównane do 4 bajtów na linię (12 bajtów w sumie)

            Thread[] threads = new Thread[3];

            // Przymocowanie wskaźnika do bitmapOutput
            {
                for (int i = 0; i < 3; i++)
                {
                    int start = i * 4; // Początek każdej linii

                    threads[i] = new Thread(() => PointerOnBitmapLine(bitmapSource, bitmapOutput, start));
                    threads[i].Start();
                }

                // Czekamy na zakończenie wszystkich wątków
                foreach (Thread t in threads)
                {
                    t.Join();
                }
            }

            // Wyświetlenie wyniku przetwarzania
            for (int i = 0; i < bitmapOutput.Length; i++)
            {
                Console.Write(bitmapOutput[i] + " ");
                if ((i + 1) % 4 == 0) Console.WriteLine(); // Nowa linia co 4 bajty
            }
        }
    }
}
