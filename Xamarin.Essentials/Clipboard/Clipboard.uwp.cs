using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using static Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        static void PlatformSetText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            SetContent(dataPackage);
        }

        static bool PlatformHasText
            => GetContent().Contains(StandardDataFormats.Text);

        static Task<string> PlatformGetTextAsync()
        {
            var clipboardContent = GetContent();
            return clipboardContent.Contains(StandardDataFormats.Text)
                ? clipboardContent.GetTextAsync().AsTask()
                : Task.FromResult<string>(null);
        }
    }
}
