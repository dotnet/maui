using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Microsoft.Maui.Essentials
{
    public static partial class TextToSpeech
    {
        internal static Task<IEnumerable<Locale>> PlatformGetLocalesAsync() =>
            Task.FromResult(SpeechSynthesizer.AllVoices.Select(v => new Locale(v.Language, null, v.DisplayName, v.Id)));

        internal static async Task PlatformSpeakAsync(string text, SpeechOptions options, CancellationToken cancelToken = default)
        {
            var tcsUtterance = new TaskCompletionSource<bool>();

            try
            {
                var player = new MediaPlayer();

                var ssml = GetSpeakParametersSSMLProsody(text, options);

                var speechSynthesizer = new SpeechSynthesizer();

                if (!string.IsNullOrWhiteSpace(options?.Locale?.Id))
                {
                    var voiceInfo = SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Id == options.Locale.Id) ?? SpeechSynthesizer.DefaultVoice;
                    speechSynthesizer.Voice = voiceInfo;
                }

                var stream = await speechSynthesizer.SynthesizeSsmlToStreamAsync(ssml);

                player.MediaEnded += PlayerMediaEnded;
                player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
                player.Play();

                void OnCancel()
                {
                    player.PlaybackSession.PlaybackRate = 0;
                    tcsUtterance.TrySetResult(true);
                }

                using (cancelToken.Register(OnCancel))
                {
                    await tcsUtterance.Task;
                }

                player.MediaEnded -= PlayerMediaEnded;
                player.Dispose();

                void PlayerMediaEnded(MediaPlayer sender, object args)
                {
                    tcsUtterance.TrySetResult(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to playback stream: " + ex);
                tcsUtterance.TrySetException(ex);
            }
        }

        static string GetSpeakParametersSSMLProsody(string text, SpeechOptions options)
        {
            var volume = "default";
            var pitch = "default";
            var rate = "default";

            // Look for the specified language, otherwise the default voice
            var locale = options?.Locale?.Language ?? SpeechSynthesizer.DefaultVoice.Language;

            if (options?.Volume.HasValue ?? false)
                volume = (options.Volume.Value * 100f).ToString(CultureInfo.InvariantCulture);

            if (options?.Pitch.HasValue ?? false)
                pitch = ProsodyPitch(options.Pitch);

            // SSML generation
            var ssml = new StringBuilder();
            ssml.AppendLine($"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='{locale}'>");
            ssml.AppendLine($"<prosody pitch='{pitch}' rate='{rate}' volume='{volume}'>{text}</prosody> ");
            ssml.AppendLine($"</speak>");

            return ssml.ToString();
        }

        static string ProsodyPitch(float? pitch)
        {
            if (!pitch.HasValue)
                return "default";

            if (pitch.Value <= 0.25f)
                return "x-low";
            else if (pitch.Value > 0.25f && pitch.Value <= 0.75f)
                return "low";
            else if (pitch.Value > 0.75f && pitch.Value <= 1.25f)
                return "medium";
            else if (pitch.Value > 1.25f && pitch.Value <= 1.75f)
                return "high";
            else if (pitch.Value > 1.75f)
                return "x-high";

            return "default";
        }
    }
}
