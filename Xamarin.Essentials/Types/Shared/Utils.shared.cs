using System;
#if !NETSTANDARD1_0
using System.Security.Cryptography;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    class Utils
    {
        internal static Version ParseVersion(string version)
        {
            if (Version.TryParse(version, out var number))
                return number;

            if (int.TryParse(version, out var major))
                return new Version(major, 0);

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

        internal static CancellationToken TimeoutToken(CancellationToken cancellationToken, TimeSpan timeout)
        {
            // create a new linked cancellation token source
            var cancelTokenSrc = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // if a timeout was given, make the token source cancel after it expires
            if (timeout > TimeSpan.Zero)
                cancelTokenSrc.CancelAfter(timeout);

            // our Cancel method will handle the actual cancellation logic
            return cancelTokenSrc.Token;
        }

        internal static async Task<T> WithTimeout<T>(Task<T> task, TimeSpan timeSpan)
        {
            var retTask = await Task.WhenAny(task, Task.Delay(timeSpan))
                .ConfigureAwait(false);

            return retTask is Task<T> ? task.Result : default(T);
        }
    }
}
