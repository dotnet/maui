using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler : ElementHandler<IApplication, IUIApplicationDelegate>
	{
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
		{
#if __MACCATALYST__
			NSApplication.SharedApplication.Terminate();
#else
			handler.Logger?.LogWarning("iOS does not support programmatically terminating the app.");
#endif
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("tvos13.0")]
		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			handler.PlatformView?.RequestNewWindow(application, args as OpenWindowRequest);
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("tvos13.0")]
		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				// See if the window's handler has an associated UIWindowScene and UISceneSession
				var sceneSession = (window.Handler?.PlatformView as UIWindow)?.WindowScene?.Session;

				if (sceneSession != null)
				{
					// Request that the scene be destroyed
					UIApplication.SharedApplication.RequestSceneSessionDestruction(sceneSession, null, errorHandler: (Foundation.NSError error) =>
					{
						handler.Logger?.LogWarning("Error during window closing. Error: {}", error);
					});
				}
			}
		}

		[SupportedOSPlatform("maccatalyst13.0")]
		public static partial void MapActivateWindow(ApplicationHandler handler, IApplication application, object? args)
		{
			if (args is IWindow window)
			{
				var sceneSession = (window.Handler?.PlatformView as UIWindow)?.WindowScene?.Session;

				if (sceneSession is not null)
				{
					UISceneSessionActivationRequest activationRequest = UISceneSessionActivationRequest.Create(sceneSession);
					UIApplication.SharedApplication.ActivateSceneSession(activationRequest, errorHandler: (Foundation.NSError error) =>
					{
						handler.Logger?.LogWarning("Error during window activation. Error: {}", error);
					});
				}
			}
		}

		internal static partial void MapAppTheme(ApplicationHandler handler, IApplication application)
		{
			application?.UpdateUserInterfaceStyle();
		}

#if __MACCATALYST__
		class NSApplication
		{
			static NativeHandle ClassHandle => ObjCRuntime.Class.GetHandle("NSApplication");
			static NativeHandle SharedApplicationSelector => ObjCRuntime.Selector.GetHandle("sharedApplication");
			static NativeHandle TerminateSelector => ObjCRuntime.Selector.GetHandle("terminate:");

			readonly NativeHandle _handle;

			NSApplication(NativeHandle handle)
			{
				_handle = handle;
			}

			public static NSApplication SharedApplication =>
				new(NativeHandle_objc_msgSend(ClassHandle, SharedApplicationSelector));

			public void Terminate() =>
				void_objc_msgSend_NativeHandle(_handle, TerminateSelector, NativeHandle.Zero);

			[DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
			static extern NativeHandle NativeHandle_objc_msgSend(NativeHandle receiver, NativeHandle selector);

			[DllImport(ObjCRuntime.Constants.ObjectiveCLibrary, EntryPoint = "objc_msgSend")]
			static extern void void_objc_msgSend_NativeHandle(NativeHandle receiver, NativeHandle selector, NativeHandle arg1);
		}
#endif
	}
}