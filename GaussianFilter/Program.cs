using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace GaussianFilter
{
    internal class Program
    {
        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll")]
        public static extern void MyProc2(ref int a, int b);

        // Dane źródłowe (bitmapa 3x3 wyrównana do 4 bajtów na linię)
        static byte[] bitmapSource = new byte[]
        {
            1, 2, 3, 0,  // Pierwsza linia (3 piksele + 1 bajt wyrównania)
            4, 5, 6, 0,  // Druga linia (3 piksele + 1 bajt wyrównania)
            7, 8, 9, 0   // Trzecia linia (3 piksele + 1 bajt wyrównania)
        };

        // Dane wyjściowe
        static byte[] bitmapOutput = new byte[12]; // Też wyrównane do 4 bajtów na linię (12 bajtów w sumie)

        // Metoda przetwarzania dla wątków
        static unsafe void ProcessBitmapPart(byte* inputPtr, byte* outputPtr, int length)
        {
            // Przetwarzamy odpowiedni fragment danych za pomocą wskaźników
            for (int i = 0; i < length; i++)
            {
                int value = *(inputPtr + i); // Pobierz wartość źródłową przez wskaźnik
                MyProc2(ref value, 12);      // Przekaż przez ref do MyProc2
                *(outputPtr + i) = (byte)value; // Zapisz wynik przez wskaźnik
            }
        }

        static unsafe void ProcessBitmapLine(byte* inputPtr, byte* outputPtr, int start, int length)
        {
            ProcessBitmapPart(inputPtr + start, outputPtr + start, length);
        }

        static unsafe void Main(string[] args)
        {
            int stride = 4; // Wyrównanie do 4 bajtów na linię
            int width = 3;  // Szerokość bitmapy (bez wyrównania)
            int height = 3; // Wysokość bitmapy

            Thread[] threads = new Thread[3];

            fixed (byte* inputPtr = bitmapSource)   // Przymocowanie wskaźnika do bitmapSource
            fixed (byte* outputPtr = bitmapOutput)  // Przymocowanie wskaźnika do bitmapOutput
            {
                // Wątek 1: linia 1 (bajty 0-3)
                // Wątek 2: linia 2 (bajty 4-7)
                // Wątek 3: linia 3 (bajty 8-11)
                for (int i = 0; i < 3; i++)
                {
                    int start = i * stride; // Początek każdej linii
                    int length = stride;    // Długość danych do przetworzenia dla każdego wątku

                    // Przekazujemy wskaźniki do nowej metody, unikając lambdy
                    threads[i] = new Thread(() => ProcessBitmapLine(inputPtr, outputPtr, start, length));
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
                if ((i + 1) % stride == 0) Console.WriteLine(); // Nowa linia co 4 bajty
            }
        }
    }
}
