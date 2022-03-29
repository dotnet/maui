using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Foundation;
using ObjCRuntime;
using UIKit;

#if __IOS__
using CoreMotion;
#elif __WATCHOS__
using CoreMotion;
#endif

using Microsoft.Maui.Essentials.Implementations;

namespace Microsoft.Maui.Essentials
{
	public static partial class Platform
	{
#if __IOS__ || __TVOS__
		public static bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
			=> WebAuthenticator.OpenUrl(new Uri(url.AbsoluteString));

		public static bool ContinueUserActivity(UIApplication application, NSUserActivity userActivity, UIApplicationRestorationHandler completionHandler)
			=> WebAuthenticator.OpenUrl(new Uri(userActivity?.WebPageUrl?.AbsoluteString));
#endif

#if __IOS__
		public static void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
		{
			if (shortcutItem.Type == AppActions.Type)
			{
				var appAction = shortcutItem.ToAppAction();

				AppActions.InvokeOnAppAction(application, shortcutItem.ToAppAction());
			}
		}
#endif

#if __IOS__
		[DllImport(Constants.SystemLibrary, EntryPoint = "sysctlbyname")]
#else
		[DllImport(Constants.libSystemLibrary, EntryPoint = "sysctlbyname")]
#endif
		internal static extern int SysctlByName([MarshalAs(UnmanagedType.LPStr)] string property, IntPtr output, IntPtr oldLen, IntPtr newp, uint newlen);

		internal static string GetSystemLibraryProperty(string property)
		{
			var lengthPtr = Marshal.AllocHGlobal(sizeof(int));
			SysctlByName(property, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0);

			var propertyLength = Marshal.ReadInt32(lengthPtr);

			if (propertyLength == 0)
			{
				Marshal.FreeHGlobal(lengthPtr);
				throw new InvalidOperationException("Unable to read length of property.");
			}

			var valuePtr = Marshal.AllocHGlobal(propertyLength);
			SysctlByName(property, valuePtr, lengthPtr, IntPtr.Zero, 0);

			var returnValue = Marshal.PtrToStringAnsi(valuePtr);

			Marshal.FreeHGlobal(lengthPtr);
			Marshal.FreeHGlobal(valuePtr);

			return returnValue;
		}

#if __IOS__ || __TVOS__

		static Func<UIViewController> getCurrentController;

		public static void Init(Func<UIViewController> getCurrentUIViewController)
			=> getCurrentController = getCurrentUIViewController;

		public static UIViewController GetCurrentUIViewController() =>
			GetCurrentViewController(false);

		internal static UIViewController GetCurrentViewController(bool throwIfNull = true)
		{
			var viewController = getCurrentController?.Invoke();

			if (viewController != null)
				return viewController;

			UIWindow window = null; 
			
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				viewController = window.RootViewController;

			if (viewController == null && !OperatingSystem.IsIOSVersionAtLeast(15))
			{
				window = UIApplication.SharedApplication
					.Windows
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);

				if (window == null && throwIfNull)
					throw new InvalidOperationException("Could not find current view controller.");
				else
					viewController = window?.RootViewController;
			}

			while (viewController?.PresentedViewController != null)
				viewController = viewController.PresentedViewController;

			if (throwIfNull && viewController == null)
				throw new InvalidOperationException("Could not find current view controller.");

			return viewController;
		}

		internal static UIWindow GetCurrentWindow(bool throwIfNull = true)
		{
			UIWindow window = null;
			
			if (!OperatingSystem.IsIOSVersionAtLeast(13))
				window = UIApplication.SharedApplication.KeyWindow;

			if (window != null && window.WindowLevel == UIWindowLevel.Normal)
				return window;

			if (window == null && !OperatingSystem.IsIOSVersionAtLeast(15))
			{
				window = UIApplication.SharedApplication
					.Windows
					.OrderByDescending(w => w.WindowLevel)
					.FirstOrDefault(w => w.RootViewController != null && w.WindowLevel == UIWindowLevel.Normal);
			}

			if (throwIfNull && window == null)
				throw new InvalidOperationException("Could not find current window.");

			return window;
		}
#endif

#if __IOS__ || __WATCHOS__
		static CMMotionManager motionManager;

		internal static CMMotionManager MotionManager =>
			motionManager ?? (motionManager = new CMMotionManager());
#endif

		internal static NSOperationQueue GetCurrentQueue() =>
			NSOperationQueue.CurrentQueue ?? new NSOperationQueue();

#if __IOS__
		internal class UIPresentationControllerDelegate : UIAdaptivePresentationControllerDelegate
		{
			Action dismissHandler;

			internal UIPresentationControllerDelegate(Action dismissHandler)
				=> this.dismissHandler = dismissHandler;

			public override void DidDismiss(UIPresentationController presentationController)
			{
				dismissHandler?.Invoke();
				dismissHandler = null;
			}

			protected override void Dispose(bool disposing)
			{
				dismissHandler?.Invoke();
				base.Dispose(disposing);
			}
		}
#endif
	}
}
