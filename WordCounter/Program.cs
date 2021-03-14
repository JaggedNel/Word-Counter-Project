#define USE_SERVICE

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

            //TestService();
            Count(@"D:\FullWarAndPeace.txt", true);
            //Tests(@"D:\WarAndPeace.txt", 5);

            Console.WriteLine("Program complete!");
        }
        
        static void TestService()
        {
            ServiceReference1.CompositeType ret;
            using (var client = new ServiceReference1.Service1Client())
            {
                ret = client.GetDataUsingDataContract(new ServiceReference1.CompositeType
                {
                    BoolValue = true,
                    StringValue = "True"
                });
            }
            Console.WriteLine($"{ret.StringValue} {ret.BoolValue}");
        }

        static void Count(string path, bool async = true)
        {
            string text = "";
            if (!Read(path, ref text))
                return;
            Dictionary<string, int> words;
            Stopwatch stopwatch = new Stopwatch();

#if USE_SERVICE
            using (var client = new ServiceReference1.Service1Client())
            {
                stopwatch.Start();
                var res = client.CountWords(text);
                stopwatch.Stop();
                if (res.ErrorMes != null)
                    throw new Exception(res.ErrorMes);
                words = res.Value;
            }
#else
            if (async)
            {
                stopwatch.Start();
                words = Counter.ConcurentParse(text,  Environment.ProcessorCount);
                stopwatch.Stop();
            }
            else
            {
                MethodInfo mi = typeof(Counter).GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);
                stopwatch.Start();
                words = (Dictionary<string, int>)mi.Invoke(null, new object[] { text });
                stopwatch.Stop();
            }
#endif

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
            TimeSpan minOneThreadTime = TimeSpan.MaxValue;
            int maxThreadsCount = 20;
            TimeSpan[,] multyThreadTime = new TimeSpan[2,maxThreadsCount];
            TimeSpan[,] minMultyThreadTime = new TimeSpan[2,maxThreadsCount];
            for (int i = 0; i < maxThreadsCount; i++)
            {
                multyThreadTime[0,i] = TimeSpan.Zero;
                minMultyThreadTime[0,i] = TimeSpan.MaxValue;
                multyThreadTime[1, i] = TimeSpan.Zero;
                minMultyThreadTime[1, i] = TimeSpan.MaxValue;
            }
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
                    if (minOneThreadTime > stopwatch.Elapsed)
                        minOneThreadTime = stopwatch.Elapsed;
                    stopwatch.Reset();

                    for (int k = 0; k < maxThreadsCount; k++)
                    {
                        stopwatch.Start();
                        Counter.AsyncParse(text, k + 1);
                        stopwatch.Stop();
                        multyThreadTime[0,k] += stopwatch.Elapsed;
                        if (minMultyThreadTime[0,k] > stopwatch.Elapsed)
                            minMultyThreadTime[0,k] = stopwatch.Elapsed;
                        stopwatch.Reset();

                        stopwatch.Start();
                        Counter.ConcurentParse(text, k + 1);
                        stopwatch.Stop();
                        multyThreadTime[1,k] += stopwatch.Elapsed;
                        if (minMultyThreadTime[1,k] > stopwatch.Elapsed)
                            minMultyThreadTime[1,k] = stopwatch.Elapsed;
                        stopwatch.Reset();
                    }
                }
            }
            Console.WriteLine("100%");
            
            Console.WriteLine($"Обработка стандартного метода заняла {oneThreadTime.TotalMilliseconds / testsDozensCount / 10} миллисекунд.");
            Console.WriteLine($"Threads| Time | Min  | Time | Min ");
            for (int i = 0; i < maxThreadsCount; i++)
                Console.WriteLine($"{i+1,7}|{multyThreadTime[0,i].TotalMilliseconds / testsDozensCount / 10,6:F2}|{minMultyThreadTime[0,i].TotalMilliseconds,6:F2}|{multyThreadTime[1, i].TotalMilliseconds / testsDozensCount / 10,6:F2}|{minMultyThreadTime[1, i].TotalMilliseconds,6:F2}");
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
