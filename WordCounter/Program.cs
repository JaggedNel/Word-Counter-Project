using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using ClassLibrary;

namespace WordCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            Count(@"D:\WarAndPeace.txt");
        }

        static void Count(string path)
        {
            Console.WriteLine("Program started!");
            string text = "";
            if (!Read(path, ref text))
                return;

            MethodInfo mi = typeof(Counter).GetMethod("Parse", BindingFlags.Static | BindingFlags.NonPublic);
            if (mi == null)
            {
                Console.WriteLine("Method not found.");
                return;
            }
            Dictionary<string, int> words = (Dictionary<string, int>)mi.Invoke(null, new object[] { text });

            StringBuilder result = new StringBuilder();
            result.Append($"{"Count",7}|Word");

            foreach (var i in words.OrderByDescending(x => x.Value))
                result.Append($"\n{i.Value,7}|{i.Key,-23}");
            Write("Reuslt.txt", result.ToString());
            Console.WriteLine("Program complete!");
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
