using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tizen.Uix.Tts;

namespace Xamarin.Essentials
{
    public static partial class TextToSpeech
    {
        static TtsClient tts = null;
        static TaskCompletionSource<bool> tcsInitialize = null;
        static TaskCompletionSource<bool> tcsUtterances = null;

        internal static async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
        {
            await Initialize();

            if (tcsUtterances?.Task != null)
                await tcsUtterances.Task;

            tcsUtterances = new TaskCompletionSource<bool>();
            if (cancelToken != null)
            {
                cancelToken.Register(() =>
                {
                    tts?.Stop();
                    tcsUtterances?.TrySetResult(true);
                });
            }

            var language = "en_US";
            var voiceType = Voice.Auto;
            if (options?.Locale.Language != null)
            {
                foreach (var voice in tts.GetSupportedVoices())
                {
                    if (voice.Language == options.Locale.Language)
                    {
                        language = voice.Language;
                        voiceType = voice.VoiceType;
                    }
                }
            }

            var pitch = 0;
            if (options?.Pitch.HasValue ?? false)
                pitch = (int)Math.Round(options.Pitch.Value / PitchMax * tts.GetSpeedRange().Max, MidpointRounding.AwayFromZero);

            tts.AddText(text, language, (int)voiceType, pitch);
            tts.Play();

            await tcsUtterances.Task;
        }

        internal static async Task<IEnumerable<Locale>> PlatformGetLocalesAsync()
        {
            await Initialize();
            var list = new List<Locale>();
            foreach (var voice in tts?.GetSupportedVoices())
                list.Add(new Locale(voice.Language, null, null, null));
            return list;
        }

        static Task<bool> Initialize()
        {
            if (tcsInitialize != null && tts != null)
                return tcsInitialize.Task;

            tcsInitialize = new TaskCompletionSource<bool>();
            tts = new TtsClient();

            tts.StateChanged += (s, e) =>
            {
                if (e.Current == State.Ready)
                    tcsInitialize?.TrySetResult(true);
            };

            tts.UtteranceCompleted += (s, e) =>
            {
                tts?.Stop();
                tcsUtterances?.TrySetResult(true);
            };

            tts.Prepare();
            return tcsInitialize.Task;
        }
    }
}
