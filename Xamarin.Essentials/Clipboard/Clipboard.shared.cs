using System;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static Task SetTextAsync(string text)
            => PlatformSetTextAsync(text ?? string.Empty);

        public static bool HasText
            => PlatformHasText;

        public static Task<string> GetTextAsync()
            => PlatformGetTextAsync();

        public static event EventHandler<EventArgs> ClipboardContentChanged
        {
            add
            {
                var wasRunning = ClipboardContentChangedInternal != null;

                ClipboardContentChangedInternal += value;

                if (!wasRunning && ClipboardContentChangedInternal != null)
                {
                    StartClipboardListeners();
                }
            }

            remove
            {
                var wasRunning = ClipboardContentChangedInternal != null;

                ClipboardContentChangedInternal -= value;

                if (wasRunning && ClipboardContentChangedInternal == null)
                    StopClipboardListeners();
            }
        }

        static event EventHandler<EventArgs> ClipboardContentChangedInternal;

        internal static void ClipboardChangedInternal() => ClipboardContentChangedInternal?.Invoke(null, EventArgs.Empty);
    }
}
