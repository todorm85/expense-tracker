using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
[assembly: InternalsVisibleTo("ExpenseTracker.Tests")]

namespace ExpenseTracker.UI
{
    internal class CategoriesJsonParser
    {
        private string LoadJsonFromFile(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                return r.ReadToEnd();
            }
        }

        public Dictionary<string, string> ParseFile(string filePath)
        {
            return this.Parse(LoadJsonFromFile(filePath));
        }

        public Dictionary<string, string> Parse(string json)
        {
            var categoriesKeyphrases = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(json);
            var keyphrasesCategory = new Dictionary<string, string>();
            foreach (var category in categoriesKeyphrases)
            {
                foreach (var keyphrase in category.Value)
                {
                    if (keyphrasesCategory.ContainsKey(keyphrase))
                    {
                        throw new ArgumentException("Duplicate keyphrase found for classifier settings parser.");
                    }

                    keyphrasesCategory.Add(keyphrase, category.Key);
                }
            }

            return keyphrasesCategory;
        }
    }
}