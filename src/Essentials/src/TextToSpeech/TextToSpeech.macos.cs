using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppKit;

namespace Microsoft.Maui.Media
{
	partial class TextToSpeechImplementation : ITextToSpeech
	{
		readonly Lazy<NSSpeechSynthesizer> speechSynthesizer = new Lazy<NSSpeechSynthesizer>(() =>
			new NSSpeechSynthesizer { Delegate = new SpeechSynthesizerDelegate() });

		Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
			Task.FromResult(NSSpeechSynthesizer.AvailableVoices
				.Select(voice => NSSpeechSynthesizer.AttributesForVoice(voice))
				.Select(attribute => new Locale(attribute["VoiceLanguage"]?.ToString(), null, attribute["VoiceName"]?.ToString(), attribute["VoiceIdentifier"]?.ToString())));

		async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
		{
			var ss = speechSynthesizer.Value;
			var ssd = (SpeechSynthesizerDelegate)ss.Delegate;

			var tcs = new TaskCompletionSource<bool>();
			try
			{
				if (options != null)
				{
					if (options.Volume.HasValue)
						ss.Volume = NormalizeVolume(options.Volume);

					if (options.Locale != null)
						ss.Voice = options.Locale.Id;

					if (options.Rate.HasValue)
						ss.Rate = options.Rate.Value;
				}

				ssd.FinishedSpeaking += OnFinishedSpeaking;
				ssd.EncounteredError += OnEncounteredError;

				ss.StartSpeakingString(text);

				using (cancelToken.Register(TryCancel))
				{
					await tcs.Task;
				}
			}
			finally
			{
				ssd.FinishedSpeaking -= OnFinishedSpeaking;
				ssd.EncounteredError -= OnEncounteredError;
			}

			void TryCancel()
			{
#pragma warning disable 0618
				// hWord is obsolete, but only just the latest release
				ss.StopSpeaking(NSSpeechBoundary.hWord);
#pragma warning restore 0618
				tcs.TrySetResult(true);
			}

			void OnFinishedSpeaking(bool completed)
			{
				tcs.TrySetResult(completed);
			}

			void OnEncounteredError(string errorMessage)
			{
				// TODO: a real exception type here
				tcs.TrySetException(new Exception(errorMessage));
			}
		}

		static float NormalizeVolume(float? volume)
		{
			var v = volume ?? 1.0f;
			if (v > 1.0f)
				v = 1.0f;
			else if (v < 0.0f)
				v = 0.0f;
			return v;
		}

		class SpeechSynthesizerDelegate : NSSpeechSynthesizerDelegate
		{
			public event Action<bool> FinishedSpeaking;

			public event Action<string> EncounteredError;

			public override void DidEncounterError(NSSpeechSynthesizer sender, nuint characterIndex, string theString, string message) =>
				EncounteredError?.Invoke(message);

			public override void DidFinishSpeaking(NSSpeechSynthesizer sender, bool finishedSpeaking) =>
				FinishedSpeaking?.Invoke(finishedSpeaking);
		}
	}
}
