using Force.Crc32;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseTracker.Core
{
    internal class Utils
    {
        internal static string ComputeSha256Hash(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
            {
                return string.Empty;
            }

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        internal static string ComputeCRC32Hash(string rawData)
        {
            return Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(rawData)).ToString();
        }
    }
}