#nullable enable
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="Type[@FullName='Microsoft.Maui.Essentials.TextToSpeech']/Docs" />
	public static partial class TextToSpeech
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='GetLocalesAsync']/Docs" />
		public static Task<IEnumerable<Locale>> GetLocalesAsync() =>
			Current.GetLocalesAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='SpeakAsync'][1]/Docs" />
		public static Task SpeakAsync(string text, CancellationToken cancelToken = default) =>
			Current.SpeakAsync(text, default, cancelToken);

		/// <include file="../../docs/Microsoft.Maui.Essentials/TextToSpeech.xml" path="//Member[@MemberName='SpeakAsync'][2]/Docs" />
		public static Task SpeakAsync(string text, SpeechOptions? options, CancellationToken cancelToken = default) =>
			Current.SpeakAsync(text, options, cancelToken);

		static ITextToSpeech Current => Media.TextToSpeech.Current;
	}
}
