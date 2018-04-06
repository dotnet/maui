using System;
#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif
using System.Text;

namespace Xamarin.Essentials
{
    internal class Utils
    {
        internal static Version ParseVersion(string version)
        {
            if (Version.TryParse(version, out var number))
                return number;

            return new Version(0, 0);
        }

        internal static string Md5Hash(string input)
        {
#if NETSTANDARD1_0
            throw new NotImplementedInReferenceAssemblyException();
#else
            var hash = new StringBuilder();
            var md5provider = new MD5CryptoServiceProvider();
            var bytes = md5provider.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (var i = 0; i < bytes.Length; i++)
                hash.Append(bytes[i].ToString("x2"));

            return hash.ToString();
#endif
        }
    }
}
