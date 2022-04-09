using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	partial class TextToSpeechImplementation : ITextToSpeech
	{
		Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;

		Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
