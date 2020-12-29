using Force.Crc32;
using System.Text;

namespace ExpenseTracker.Core
{
    internal class Utils
    {
        internal static string ComputeCRC32Hash(string rawData)
        {
            return Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(rawData)).ToString();
        }
    }
}