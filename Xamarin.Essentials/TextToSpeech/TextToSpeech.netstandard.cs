using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        internal static Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default) =>
            throw ExceptionUtils.NotSupportedOrImplementedException;

        internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
            throw ExceptionUtils.NotSupportedOrImplementedException;
    }
}
