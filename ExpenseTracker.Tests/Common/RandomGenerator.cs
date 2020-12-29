using System;

namespace ExpenseTracker.Tests.Common
{
    public class RandomGenerator
    {
        public string GetDate(string format = "dd.MM.yyyy")
        {
            return new DateTime(GetRandom(2000, 2010), GetRandom(1, 12), GetRandom(1, 29)).ToString(format);
        }

        public int GetRandom(int min = 1, int max = 12)
        {
            return new Random().Next(min, max + 1);
        }
    }
}