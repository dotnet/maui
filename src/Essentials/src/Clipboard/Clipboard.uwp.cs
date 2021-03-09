using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using WindowsClipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Microsoft.Maui.Essentials
{
    public static partial class Clipboard
    {
        static Task PlatformSetTextAsync(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            WindowsClipboard.SetContent(dataPackage);
            return Task.CompletedTask;
        }

        static bool PlatformHasText
            => WindowsClipboard.GetContent().Contains(StandardDataFormats.Text);

        static Task<string> PlatformGetTextAsync()
        {
            var clipboardContent = WindowsClipboard.GetContent();
            return clipboardContent.Contains(StandardDataFormats.Text)
                ? clipboardContent.GetTextAsync().AsTask()
                : Task.FromResult<string>(null);
        }

        static void StartClipboardListeners()
            => WindowsClipboard.ContentChanged += ClipboardChangedEventListener;

        static void StopClipboardListeners()
            => WindowsClipboard.ContentChanged -= ClipboardChangedEventListener;

        static void ClipboardChangedEventListener(object sender, object val) => ClipboardChangedInternal();
    }
}
