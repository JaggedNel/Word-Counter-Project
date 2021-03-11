using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using ClassLibrary;
using System.Diagnostics;

namespace WordCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Program started!");

            Count(@"D:\FullWarAndPeace.txt", true);
            //Tests(@"D:\WarAndPeace.txt", 5);

            Console.WriteLine("Program complete!");
        }

        static void Count(string path, bool async = false)
        {
            string text = "";
            if (!Read(path, ref text))
                return;
            Dictionary<string, int> words;
            Stopwatch stopwatch = new Stopwatch();

            if (async)
            {
                stopwatch.Start();
                words = Counter.AsyncParse(text, 3);
                stopwatch.Stop();
            }
            else
            {
                MethodInfo mi = typeof(Counter).GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);
                stopwatch.Start();
                words = (Dictionary<string, int>)mi.Invoke(null, new object[] { text });
                stopwatch.Stop();
            }

            Console.WriteLine($"Elapsed execution method time: {stopwatch.Elapsed.TotalMilliseconds} milliseconds.");
            StringBuilder result = new StringBuilder();
            result.Append($"{"Count",7}|Word");

            foreach (var i in words.OrderByDescending(x => x.Value))
                result.Append($"\n{i.Value,7}|{i.Key,-23}");
            Write("Reuslt.txt", result.ToString());
        }

        static void Tests(string path, int testsDozensCount)
        {
            Console.WriteLine($"Test started.\nFile: {path}\nCount of tests: {testsDozensCount * 10}\nStatus:");
            string text = "";
            if (!Read(path, ref text))
                return;

            TimeSpan oneThreadTime = TimeSpan.Zero;
            int maxThreadsCount = 20;
            TimeSpan[] multyThreadTime = new TimeSpan[maxThreadsCount];
            for (int i = 0; i < maxThreadsCount; i++)
                multyThreadTime[i] = TimeSpan.Zero;
            MethodInfo mi = typeof(Counter).GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);

            Stopwatch stopwatch = new Stopwatch();

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"{i * 10}%");
                for (int j = 0; j < testsDozensCount; j++)
                {
                    stopwatch.Start();
                    mi.Invoke(null, new object[] { text });
                    stopwatch.Stop();
                    oneThreadTime += stopwatch.Elapsed;
                    stopwatch.Reset();

                    for (int k = 0; k < multyThreadTime.Length; k++)
                    {
                        stopwatch.Start();
                        Counter.AsyncParse(text, k + 1);
                        stopwatch.Stop();
                        multyThreadTime[k] += stopwatch.Elapsed;
                        stopwatch.Reset();
                    }
                }
            }
            Console.WriteLine("100%");
            
            Console.WriteLine($"Обработка стандартного метода заняла {oneThreadTime.TotalMilliseconds / testsDozensCount / 10} миллисекунд.");
            for (int i = 0; i < multyThreadTime.Length; i++)
                Console.WriteLine($"Обработка {i+1}-поточного метода заняла {multyThreadTime[i].TotalMilliseconds / testsDozensCount / 10} миллисекунд.");
        }

        static bool Read(string path, ref string text)
        {
            try
            {
                using (var sr = new System.IO.StreamReader(path))
                {
                    text = sr.ReadToEnd();
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine($"The file '{path}' could not be read:");
                Console.WriteLine(e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        static bool Write(string path, string text)
        {
            try
            {
                using (System.IO.StreamWriter op = new System.IO.StreamWriter(path))
                {
                    op.Write(text);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
