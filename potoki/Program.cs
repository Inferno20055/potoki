using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using static System.Console;
namespace potoki
{
    internal class Program
    {
        static int[] numbers = new int[10000]; 
        static int maxValue;
        static int minValue;
        static double averageValue;
        static void Main(string[] args)
        {
            /*Write("Введите количество потоков: ");
            int countThreads;
            while (!int.TryParse(ReadLine(), out countThreads) || countThreads <= 0)
            {
                Write("Некорректный ввод. Попробуйте ещё раз: ");
            }

            // Ввод начального числа диапазона
            Write("Введите начальное число диапазона: ");
            int start;
            while (!int.TryParse(ReadLine(), out start))
            {
                Write("Некорректный ввод. Попробуйте ещё раз: ");
            }

            // Ввод конечного числа диапазона
            Write("Введите конечное число диапазона: ");
            int end;
            while (!int.TryParse(ReadLine(), out end))
            {
                Write("Некорректный ввод. Попробуйте ещё раз: ");
            }

            int totalNumbers = end - start + 1;
            int numbersPerThread = totalNumbers / countThreads;
            int remaining = totalNumbers % countThreads;

            Thread[] threads = new Thread[countThreads];

            int currentStart = start;

            for (int i = 0; i < countThreads; i++)
            {
                int localStart = currentStart;
                int localEnd = localStart + numbersPerThread - 1;
                if (i == countThreads - 1)
                {
                    localEnd += remaining;
                }

                threads[i] = new Thread(() => DisplayNumbers(localStart, localEnd));
                threads[i].Start();

                currentStart = localEnd + 1;
            }

            foreach (var t in threads)
            {
                t.Join();
            }

            WriteLine("\nВсе потоки завершили работу.");*/

            //задание 4
            Random rnd = new Random();
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = rnd.Next(0, 100000);
            }

            Thread maxThread = new Thread(FindMax);
            Thread minThread = new Thread(FindMin);
            Thread averageThread = new Thread(FindAverage);

            maxThread.Start();
            minThread.Start();
            averageThread.Start();

            Thread writeThread = new Thread(() => WriteResultsToFile());
            writeThread.Start();

            maxThread.Join();
            minThread.Join();
            averageThread.Join();
            writeThread.Join();

            WriteLine($"Максимальное значение: {maxValue}");
            WriteLine($"Минимальное значение: {minValue}");
            WriteLine($"Среднее значение: {averageValue:F2}");
        }
        static void DisplayNumbers(int start, int end)
        {
            //версия 3 задания
            /*for (int i = start; i <= end; i++)
            {
                WriteLine(i);
                Thread.Sleep(1000); 
            }*/
            //версия задания 4
            for (int i = start; i <= end; i++)
            {
                WriteLine($"Поток {Thread.CurrentThread.ManagedThreadId}: {i}");
                Thread.Sleep(1000); 
            }

        }
        static void FindMax()
        {
            int max = numbers[0];
            foreach (var num in numbers)
            {
                if (num > max)
                {
                    max = num;
                }
            }
            maxValue = max;
        }

        static void FindMin()
        {
            int min = numbers[0];
            foreach (var num in numbers)
            {
                if (num < min)
                {
                    min = num;
                }
            }
            minValue = min;
        }

        static void FindAverage()
        {
            long sum = 0;
            foreach (var num in numbers)
            {
                sum += num;
            }
            averageValue = (double)sum / numbers.Length;
        }

        static void WriteResultsToFile()
        {
            string filename = "results.txt";
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("Набор чисел:");
                for (int i = 0; i < numbers.Length; i++)
                {
                    writer.Write($"{numbers[i]} ");
                    if ((i + 1) % 20 == 0)
                        writer.WriteLine();
                }
                writer.WriteLine();
                writer.WriteLine($"Максимальное значение: {maxValue}");
                writer.WriteLine($"Минимальное значение: {minValue}");
                writer.WriteLine($"Среднее значение: {averageValue:F2}");
            }
            WriteLine($"Результаты записаны в файл {filename}");
        }
    }
}
