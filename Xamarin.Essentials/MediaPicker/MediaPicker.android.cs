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

                OnMediaPicked(new MediaPickedEventArgs(imagePath));
            }
            else if (requestCode == (int)RequestCode.TakePhoto || requestCode == (int)RequestCode.TakeVideo)
            {
            }
            else
            {
                // TODO: should we do something here?
            }
        }

        static Task PlatformShowPhotoPickerAsync(MediaPickerOptions options)
        {
            // TODO: request read external storage permission

            var activity = Platform.GetCurrentActivity(true);

            var intent = new Intent(Intent.ActionPick);
            intent.SetType("image/*");

            activity.StartActivityForResult(intent, (int)RequestCode.PickPhoto);

            return Task.CompletedTask;
        }
    }
}
