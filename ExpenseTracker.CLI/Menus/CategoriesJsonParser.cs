using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ExpenseTracker.Tests")]

namespace ExpenseTracker.Core.UI
{
    internal class CategoriesJsonParser
    {
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

        public Dictionary<string, string> ParseFile(string filePath)
        {
            return this.Parse(LoadJsonFromFile(filePath));
        }

        private string LoadJsonFromFile(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                return r.ReadToEnd();
            }
        }
    }
}