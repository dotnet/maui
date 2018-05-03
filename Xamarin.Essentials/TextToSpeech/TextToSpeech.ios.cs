using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AVFoundation;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
            Task.FromResult(AVSpeechSynthesisVoice.GetSpeechVoices()
                .Select(v => new Locale(v.Language, null, v.Language, v.Identifier)));

        internal static Task PlatformSpeakAsync(string text, SpeakSettings settings, CancellationToken cancelToken = default)
        {
            var speechUtterance = GetSpeechUtterance(text, settings);
            return SpeakUtterance(speechUtterance, cancelToken);
        }

        private static AVSpeechUtterance GetSpeechUtterance(string text, SpeakSettings settings)
        {
            var speechUtterance = new AVSpeechUtterance(text);

            if (settings != null)
            {
                // null voice if fine - it is the default
                speechUtterance.Voice =
                    AVSpeechSynthesisVoice.FromLanguage(settings.Locale.Language) ??
                    AVSpeechSynthesisVoice.FromLanguage(AVSpeechSynthesisVoice.CurrentLanguageCode);

                // the platform has a range of 0.5 - 2.0
                // anything lower than 0.5 is set to 0.5
                if (settings.Pitch.HasValue)
                    speechUtterance.PitchMultiplier = settings.Pitch.Value;

                if (settings.Volume.HasValue)
                    speechUtterance.Volume = settings.Volume.Value;
            }

            return speechUtterance;
        }

        internal static async Task SpeakUtterance(AVSpeechUtterance speechUtterance, CancellationToken cancelToken)
        {
            var tcsUtterance = new TaskCompletionSource<bool>();
            var speechSynthesizer = new AVSpeechSynthesizer();
            try
            {
                speechSynthesizer.DidFinishSpeechUtterance += OnFinishedSpeechUtterance;
                speechSynthesizer.SpeakUtterance(speechUtterance);
                using (cancelToken.Register(TryCancel))
                {
                    await tcsUtterance.Task;
                }
            }
            finally
            {
                speechSynthesizer.DidFinishSpeechUtterance -= OnFinishedSpeechUtterance;
            }

            void TryCancel()
            {
                speechSynthesizer?.StopSpeaking(AVSpeechBoundary.Word);
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
