using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace WcfService
{
    // ПРИМЕЧАНИЕ. Команду "Переименовать" в меню "Рефакторинг" можно использовать для одновременного изменения имени класса "Service1" в коде, SVC-файле и файле конфигурации.
    // ПРИМЕЧАНИЕ. Чтобы запустить клиент проверки WCF для тестирования службы, выберите элементы Service1.svc или Service1.svc.cs в обозревателе решений и начните отладку.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public RequestRes<Dictionary<string, int>> CountWords(string text)
        {
            try
            {
                if (text.Length > 200)
                {
                    int threadsCount = Environment.ProcessorCount;
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

                    return new RequestRes<Dictionary<string, int>>(new Dictionary<string, int>(words), null);
                }
                else
                {
                    var words = new ConcurrentDictionary<string, int>();
                    ConcurentWorker worker = new ConcurentWorker(-1, text.Length, text, words);
                    worker.Parse();
                    return new RequestRes<Dictionary<string, int>>(new Dictionary<string, int>(words), null);
                }
            }
            catch (Exception e)
            {
                return new RequestRes<Dictionary<string, int>>(null, e.Message);
            }
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
