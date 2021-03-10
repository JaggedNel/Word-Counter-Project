using System;
using System.Collections.Generic;
using System.Text;

namespace ClassLibrary
{
    public class Counter
    {
        static Dictionary<string, int> Parse(string text)
        {
            var words = new Dictionary<string, int>();
            StringBuilder word = new StringBuilder();
            foreach (var symbol in text)
            {
                if (char.IsLetter(symbol))
                {
                    word.Append(char.ToUpper(symbol));
                }
                else
                {
                    if (word.Length != 0)
                    {
                        if (words.ContainsKey(word.ToString()))
                            words[word.ToString()]++;
                        else
                            words.Add(word.ToString(), 1);
                        word.Clear();
                    }
                }
            }
            return words;
        }
    }
}
