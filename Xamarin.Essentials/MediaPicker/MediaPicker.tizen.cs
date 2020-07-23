using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        static async Task<MediaPickerResult> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
            => new MediaPickerResult(await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images
            }));
    }
}
