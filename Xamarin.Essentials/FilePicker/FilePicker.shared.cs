using System.IO;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class FilePicker
    {
        public static Task<PickResult> PickFileAsync(PickOptions options = null)
            => PlatformPickFileAsync(options ?? PickOptions.Default);
    }

    public partial class PickOptions
    {
        public static PickOptions Default
        {
            get
            {
                return new PickOptions
                {
                    FileTypes = null,
                };
            }
        }

        public static PickOptions Images
            => PlatformGetImagesPickOptions();

        public string PickerTitle { get; set; }

        public string[] FileTypes { get; set; }
    }

    public partial class PickResult
    {
        public string FileUri { get; internal set; }

        public string FileName { get; internal set; }

        public Stream GetStream()
            => PlatformGetStream();

        internal PickResult(string fileUri, string fileName)
        {
            FileUri = fileUri;
            FileName = fileName;
        }
    }
}
