using System;
using System.Security.Cryptography;
using System.Text;

namespace Frodo.Common
{
    public static class Md5Encryption
    {
        public static string Md5(this string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hash = md5.ComputeHash(inputBytes);

                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
    }
}