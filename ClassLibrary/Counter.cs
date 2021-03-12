using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace ClassLibrary
{
    public class Counter
    {
        static Dictionary<string, int> Parse(string text)
        {
            var words = new Dictionary<string, int>();
            StringBuilder word = new StringBuilder();
            string w;
            int i = -1;
            while (i < text.Length)
            {
                do
                    if (++i >= text.Length)
                        goto Finish;
                while (!char.IsLetter(text[i]));
                do
                    word.Append(char.ToUpper(text[i]));
                while (++i < text.Length && char.IsLetter(text[i]));

                w = word.ToString();
                if (words.ContainsKey(w))
                    words[w]++;
                else
                    words.Add(w, 1);
                word.Clear();
            }
            Finish:
            return words;
        }

        public static Dictionary<string, int> AsyncParse(string text, int threadsCount)
        {
            var words = new Dictionary<string, int>();
            object locker = new object(); // блокировщик состояния словаря
            int[] borders = new int[threadsCount + 1];
            borders[0] = -1;
            for (int i = 1; i < threadsCount; i++)
            {
                int n = borders[i - 1] + text.Length / threadsCount + 1;
                while (!char.IsLetter(text[n]))
                    n++;
                borders[i] = n;
            }
            borders[threadsCount] = text.Length;

            Worker[] workers = new Worker[threadsCount];
            Action[] actions = new Action[threadsCount];
            for (int i = 0; i < threadsCount; i++)
            {
                workers[i] = new Worker(borders[i], borders[i+1], text, words, locker);
                actions[i] = workers[i].Parse;
            }
            Parallel.Invoke(actions);
            
            return words;
        }
        
        class Worker
        {
            readonly int a; // Начало отрезка
            readonly int b; // Конец отрезка
            readonly string text;
            Dictionary<string, int> words;
            object locker;

            public Worker(int a, int b, string text, Dictionary<string, int> words, object locker)
            {
                this.a = a;
                this.b = b;
                this.text = text;
                this.words = words;
                this.locker = locker;
            }

            public void Parse()
            {
                StringBuilder word = new StringBuilder();
                string w;
                int i = a;
                while (i < b)
                {
                    do
                        if (++i >= b)
                            goto Finish;
                    while (!char.IsLetter(text[i]));
                    do
                        word.Append(char.ToUpper(text[i]));
                    while (++i < b && char.IsLetter(text[i]));

                    w = word.ToString();
                    lock (locker)
                        if (words.ContainsKey(w))
                            words[w]++;
                        else
                            words.Add(w, 1);
                    word.Clear();
                }
                Finish: { }
            }
        }

        public static Dictionary<string, int> ConcurentParse(string text, int threadsCount)
        {
            var words = new ConcurrentDictionary<string, int>(threadsCount, 64);
            int[] borders = new int[threadsCount + 1];
            borders[0] = -1;
            for (int i = 1; i < threadsCount; i++)
            {
                int n = borders[i - 1] + text.Length / threadsCount + 1;
                while (!char.IsLetter(text[n]))
                    n++;
                borders[i] = n;
            }
            borders[threadsCount] = text.Length;

            ConcurentWorker[] workers = new ConcurentWorker[threadsCount];
            Action[] actions = new Action[threadsCount];
            for (int i = 0; i < threadsCount; i++)
            {
                workers[i] = new ConcurentWorker(borders[i], borders[i + 1], text, words);
                actions[i] = workers[i].Parse;
            }
            Parallel.Invoke(actions);

            return new Dictionary<string, int>(words);
        }

        class ConcurentWorker
        {
            readonly int a; // Начало отрезка
            readonly int b; // Конец отрезка
            readonly string text;
            ConcurrentDictionary<string, int> words;

            public ConcurentWorker(int a, int b, string text, ConcurrentDictionary<string, int> words)
            {
                this.a = a;
                this.b = b;
                this.text = text;
                this.words = words;
            }

            public void Parse()
            {
                StringBuilder word = new StringBuilder();
                string w;
                int i = a;
                while (i < b)
                {
                    do
                        if (++i >= b)
                            goto Finish;
                    while (!char.IsLetter(text[i]));
                    do
                        word.Append(char.ToUpper(text[i]));
                    while (++i < b && char.IsLetter(text[i]));

                    w = word.ToString();
                    words.AddOrUpdate(w, 1, (_, x) => ++x);
                    word.Clear();
                }
                Finish: { }
            }
        }
    }
}
