using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Provider;

namespace Xamarin.Essentials
{
    public static partial class MediaPicker
    {
        enum RequestCode
        {
            Base = 10010,

            PickPhoto = Base + 1,
            TakePhoto = Base + 2,
            PickVideo = Base + 3,
            TakeVideo = Base + 4,

            Min = PickPhoto,
            Max = TakeVideo,
        }

        static MediaPicker()
        {
            Platform.ActivityResult += OnActivityResult;
        }

        static void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // this wasn't us
            if (requestCode < (int)RequestCode.Min || requestCode > (int)RequestCode.Max)
                return;

            // the user canceled
            if (resultCode == Result.Canceled)
            {
                OnMediaPicked(new MediaPickedEventArgs(true));
                return;
            }

            // an OK, so process
            var args = ProcessPickerIntent(requestCode, data);

            // raise the global event
            OnMediaPicked(args);
        }

        static async Task<MediaFile> PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            // make sure we have permission and an activity
            await Permissions.RequireAsync(PermissionType.ExternalStorage);

            var intent = new Intent(Intent.ActionPick);
            intent.SetType("image/*");

            try
            {
                // launch the picker intent via the intermediate activity
                var data = await IntermediateActivity.StartAsync(intent, (int)RequestCode.PickPhoto);

                // process the task response
                var result = ProcessPickerIntent((int)RequestCode.PickPhoto, data);
                return result.File;
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        static MediaPickedEventArgs ProcessPickerIntent(int requestCode, Intent data)
        {
            // this is a result of a pick
            if (requestCode == (int)RequestCode.PickPhoto || requestCode == (int)RequestCode.PickVideo)
            {
                string imagePath = null;

                var imageUri = data?.Data;
                if (imageUri.Scheme == "file")
                {
                    imagePath = imageUri.GetAbsolutePath();
                }
                else if (imageUri.Scheme == "content")
                {
                    var projection = new[] { MediaStore.MediaColumns.Data };
                    using (var cursor = Platform.AppContext.ContentResolver.Query(imageUri, projection, null, null, null))
                    {
                        if (cursor?.MoveToFirst() == true)
                        {
                            var idx = cursor.GetColumnIndex(projection[0]);
                            imagePath = cursor.GetString(idx);
                        }
                    }
                }

                return new MediaPickedEventArgs(new MediaFile(imagePath));
            }

            // this is a result of a camera operation
            if (requestCode == (int)RequestCode.TakePhoto || requestCode == (int)RequestCode.TakeVideo)
            {
            }

            // something went wrong
            return new MediaPickedEventArgs(null);
        }
    }

    public partial class MediaFile
    {
        public MediaFile(string file)
        {
            FilePath = file;
        }

        Task<Stream> PlatformOpenReadAsync() =>
            Task.FromResult((Stream)File.OpenRead(FilePath));
    }
}
