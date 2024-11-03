using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace GaussianFilter
{
    internal class Program
    {
        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Asm.dll", EntryPoint = "MyProc2", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void MyProc2(byte* ptr, byte valueToAdd);

        [DllImport(@"C:\Users\Kamil\Desktop\Projekt_JA_filtrGaussa\GaussianFilter\x64\Debug\Cdll.dll", EntryPoint = "calculateFilterCPP", CallingConvention = CallingConvention.StdCall)]
        public static extern unsafe void calculateFilterCPP(byte* ptr, byte valueToAdd);
        public static bool asmOrC = false;

        public static bool IsPowerOfTwo(int number)
        {
            return number > 0 && (number & (number - 1)) == 0;
        }

        public static int numbersToPowerOfTwo(int number)
        {
            int result = 0;
            while (!IsPowerOfTwo(number))
            {
                number++;
                result++;
            }
            return result;
        }

        static unsafe void ProcessBitmap(byte* inputPtr, byte* outputPtr, int liczbyDoZmiany)
        {
            for (int i = 0; i < liczbyDoZmiany; i++)
            {   
                byte* value = inputPtr + i;
                if(!asmOrC)
                    MyProc2(value, 3);
                else calculateFilterCPP(value, 10);
                *(outputPtr + i) = *value; 
            }
        }

        static unsafe void PointerOnBitmapLine(byte[] bitmapSource, byte[] bitmapOutput, int start, int liczbyDoZmiany)
        {
            fixed (byte* inputPtr = bitmapSource)   
            fixed (byte* outputPtr = bitmapOutput)
            {
                ProcessBitmap(inputPtr + start, outputPtr + start, liczbyDoZmiany);
            }
               
        }

        static unsafe void Main(string[] args)
        {
            string filePath = "C:\\Users\\Kamil\\Desktop\\Projekt_JA_filtrGaussa\\GaussianFilter\\bitmapa.txt"; // Ścieżka do pliku z liczbami
            for(int j =0; j < 2; j++)
            {
                try
                {
                    // Wczytywanie całej zawartości pliku jako jednego ciągu znaków
                    string zawartoscPliku = File.ReadAllText(filePath);

                    // Podzielenie zawartości na części po spacjach
                    string[] liczbyString = zawartoscPliku.Split(' ');

                    // Inicjalizacja tablicy o rozmiarze równym liczbie elementów w pliku
                    int iloscliczb = liczbyString.Length;
                    Console.WriteLine($"liczba: {iloscliczb}");
                    double pierwiastek = Math.Sqrt(iloscliczb);
                    int amountOfNumbers = numbersToPowerOfTwo((int)pierwiastek);
                    iloscliczb += (int)pierwiastek * amountOfNumbers;
                    byte[] bitmapSource = new byte[iloscliczb];
                    int index = 0;
                    int index2 = 1;
                    Console.WriteLine($"pierwiastek: {pierwiastek}");

                    // Konwersja każdego elementu tekstowego na liczbę
                    foreach (var liczbaString in liczbyString)
                    {
                        if (byte.TryParse(liczbaString, out byte liczba))
                        {
                            bitmapSource[index] = liczba;
                            if (index2 % pierwiastek == 0)
                            {
                                for (int i = 0; i < amountOfNumbers; i++)
                                {
                                    index++;
                                    bitmapSource[index] = 0;
                                }
                            }

                            index++;
                            index2++;
                        }
                        else
                        {
                            Console.WriteLine($"Element '{liczbaString}' nie jest liczbą i został pominięty.");
                        }
                    }

                    for (int i = 0; i < bitmapSource.Length; i++)
                    {
                        Console.Write(bitmapSource[i] + " ");
                        if ((i + 1) % (pierwiastek + amountOfNumbers) == 0) Console.WriteLine();
                    }

                    byte[] bitmapOutput = new byte[iloscliczb];

                    Thread[] threads = new Thread[(int)pierwiastek];
                    int threadsCount = (int)pierwiastek;

                    Stopwatch stopwatch = new Stopwatch();

                    // Rozpocznij pomiar czasu
                    stopwatch.Start();

                    for (int i = 0; i < threadsCount; i++)
                    {
                        int start = i * (bitmapSource.Length / (int)pierwiastek);
                        threads[i] = new Thread(() => PointerOnBitmapLine(bitmapSource, bitmapOutput, start, (int)pierwiastek));
                        threads[i].Start();
                    }

                    foreach (Thread t in threads)
                    {
                        t.Join();
                    }
                    stopwatch.Stop();
                    Console.WriteLine();
                    for (int i = 0; i < bitmapOutput.Length; i++)
                    {
                        Console.Write(bitmapOutput[i] + " ");
                        if ((i + 1) % (pierwiastek + amountOfNumbers) == 0) Console.WriteLine();
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Czas wykonania: {stopwatch.ElapsedMilliseconds} ms");
                    asmOrC = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wystąpił błąd podczas odczytu pliku: {ex.Message}");
                }
            }
        }
    }
}
