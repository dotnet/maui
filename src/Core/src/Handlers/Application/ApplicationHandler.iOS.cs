using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, UIApplicationDelegate>
	{
		public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
#if __MACCATALYST__
			NSApplication.SharedApplication.Terminate();
#else
			handler.Logger?.LogWarning("iOS does not support programmatically terminating the app.");
#endif
		}

		public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.NativeView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				// See if the window's handler has an associated UIWindowScene and UISceneSession
				var sceneSession = (window.Handler?.NativeView as UIWindow)?.WindowScene?.Session;

				if (sceneSession != null)
				{
					// Request that the scene be destroyed
					// TODO: Error handler?
					UIApplication.SharedApplication.RequestSceneSessionDestruction(sceneSession, null, null);
				}
			}
		}

#if __MACCATALYST__
		class NSApplication
		{
			static IntPtr ClassHandle => ObjCRuntime.Class.GetHandle("NSApplication");
			static IntPtr SharedApplicationSelector => ObjCRuntime.Selector.GetHandle("sharedApplication");
			static IntPtr TerminateSelector => ObjCRuntime.Selector.GetHandle("terminate:");

			readonly IntPtr _handle;

			NSApplication(IntPtr handle)
			{
				_handle = handle;
			}

			public static NSApplication SharedApplication =>
				new(IntPtr_objc_msgSend(ClassHandle, SharedApplicationSelector));

			public void Terminate() =>
				void_objc_msgSend_IntPtr(_handle, TerminateSelector, IntPtr.Zero);

			[DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
			static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

			[DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
			static extern void void_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);
		}
#endif
	}
}