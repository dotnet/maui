using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

using static Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Xamarin.Essentials
{
    public static partial class Clipboard
    {
        public static void SetText(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            SetContent(dataPackage);
        }

        public static bool HasText
            => GetContent().Contains(StandardDataFormats.Text);

        public static Task<string> GetTextAsync()
        {
            var clipboardContent = GetContent();
            return clipboardContent.Contains(StandardDataFormats.Text)
                ? clipboardContent.GetTextAsync().AsTask()
                : Task.FromResult<string>(null);
        }
    }
}
