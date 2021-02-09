using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;

namespace Xamarin.Essentials
{
	public static partial class TextToSpeech
	{
		static readonly Lazy<AVSpeechSynthesizer> speechSynthesizer = new Lazy<AVSpeechSynthesizer>();

		internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
			Task.FromResult(AVSpeechSynthesisVoice.GetSpeechVoices()
				.Select(v => new Locale(v.Language, null, v.Name, v.Identifier)));

		internal static async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
		{
			using (var speechUtterance = GetSpeechUtterance(text, options))
			{
				await SpeakUtterance(speechUtterance, cancelToken);
			}
		}

		static AVSpeechUtterance GetSpeechUtterance(string text, SpeechOptions options)
		{
			var speechUtterance = new AVSpeechUtterance(text);

			if (options != null)
			{
				// null voice if fine - it is the default
				speechUtterance.Voice =
					AVSpeechSynthesisVoice.FromLanguage(options.Locale?.Language) ??
					AVSpeechSynthesisVoice.FromLanguage(AVSpeechSynthesisVoice.CurrentLanguageCode);

				// the platform has a range of 0.5 - 2.0
				// anything lower than 0.5 is set to 0.5
				if (options.Pitch.HasValue)
					speechUtterance.PitchMultiplier = options.Pitch.Value;

				if (options.Volume.HasValue)
					speechUtterance.Volume = options.Volume.Value;
			}

			return speechUtterance;
		}

		internal static async Task SpeakUtterance(AVSpeechUtterance speechUtterance, CancellationToken cancelToken)
		{
			var tcsUtterance = new TaskCompletionSource<bool>();
			try
			{
				// Ensures linker doesn't remove.
				if (DateTime.UtcNow.Ticks < 0)
					new AVSpeechSynthesizer();

				speechSynthesizer.Value.DidFinishSpeechUtterance += OnFinishedSpeechUtterance;
				speechSynthesizer.Value.SpeakUtterance(speechUtterance);
				using (cancelToken.Register(TryCancel))
				{
					await tcsUtterance.Task;
				}
			}
			finally
			{
				speechSynthesizer.Value.DidFinishSpeechUtterance -= OnFinishedSpeechUtterance;
			}

			void TryCancel()
			{
				speechSynthesizer.Value?.StopSpeaking(AVSpeechBoundary.Immediate);
				tcsUtterance?.TrySetResult(true);
			}

			void OnFinishedSpeechUtterance(object sender, AVSpeechSynthesizerUteranceEventArgs args)
			{
				if (speechUtterance == args.Utterance)
					tcsUtterance?.TrySetResult(true);
			}
		}
	}
}
