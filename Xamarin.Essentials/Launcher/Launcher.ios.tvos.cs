using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class Launcher
    {
        static Task<bool> PlatformCanOpenAsync(Uri uri) =>
            Task.FromResult(UIApplication.SharedApplication.CanOpenUrl(GetNativeUrl(uri)));

        static Task PlatformOpenAsync(Uri uri) =>
            UIApplication.SharedApplication.OpenUrlAsync(GetNativeUrl(uri), new UIApplicationOpenUrlOptions());

        static Task<bool> PlatformTryOpenAsync(Uri uri)
        {
            var nativeUrl = GetNativeUrl(uri);
            var canOpen = UIApplication.SharedApplication.CanOpenUrl(nativeUrl);

            if (canOpen)
            {
                if (Platform.HasOSVersion(10, 0))
                    return UIApplication.SharedApplication.OpenUrlAsync(nativeUrl, new UIApplicationOpenUrlOptions());

                UIApplication.SharedApplication.OpenUrl(nativeUrl);
            }

            return Task.FromResult(canOpen);
        }

        internal static NSUrl GetNativeUrl(Uri uri)
        {
            try
            {
                return new NSUrl(uri.OriginalString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to create Url from Original string, try absolute Uri: " + ex.Message);
                return new NSUrl(uri.AbsoluteUri);
            }
        }

#if __IOS__
        static UIDocumentInteractionController documentController;

        static Task PlatformOpenAsync(OpenFileRequest request)
        {
            var fileUrl = NSUrl.FromFilename(request.File.FullPath);

            documentController = UIDocumentInteractionController.FromUrl(fileUrl);
            documentController.Delegate = new DocumentControllerDelegate
            {
                DismissHandler = () =>
                {
                    documentController?.Dispose();
                    documentController = null;
                }
            };
            documentController.Uti = request.File.ContentType;

            var vc = Platform.GetCurrentViewController();

            CoreGraphics.CGRect? rect = null;
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet)
            {
                rect = new CoreGraphics.CGRect(new CoreGraphics.CGPoint(vc.View.Bounds.Width / 2, vc.View.Bounds.Height), CoreGraphics.CGRect.Empty.Size);
            }
            else
            {
                rect = vc.View.Bounds;
            }

            documentController.PresentOpenInMenu(rect.Value, vc.View, true);

            return Task.CompletedTask;
        }

        class DocumentControllerDelegate : UIDocumentInteractionControllerDelegate
        {
            public Action DismissHandler { get; set; }

            public override void DidDismissOpenInMenu(UIDocumentInteractionController controller)
                => DismissHandler?.Invoke();
        }
#else
        static Task PlatformOpenAsync(OpenFileRequest request) =>
            throw new FeatureNotSupportedException();
#endif
    }
}
