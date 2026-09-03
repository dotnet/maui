using System;
using System.Runtime.InteropServices;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.DeviceTests;

public static class UIWindowExtensions
{
	public static void SetFrame(this UIWindow window, Rect frame, bool display = true, bool animate = true)
	{
		var nsWindow = UINSWindow.From(window);
		if (nsWindow is null)
			throw new InvalidOperationException("Unable to update frame of non-existant window.");

		nsWindow.SetFrame(frame, display, animate);
	}

	public static bool HasNSWindow(this UIWindow window)
	{
		var nsWindow = UINSWindow.From(window);
		return nsWindow is not null;
	}

	class UINSWindow
	{
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void void_objc_msgSend_CGRect_bool_bool(IntPtr receiver, IntPtr selector, CGRect arg1, bool arg2, bool arg3);

		static NativeHandle? nsApplicationHandle;
		static NativeHandle NSApplicationHandle =>
			nsApplicationHandle ??= Class.GetHandle("NSApplication");

		static Selector? sharedApplicationSelector;
		static Selector SharedApplicationSelector =>
			sharedApplicationSelector ??= new Selector("sharedApplication");

		static Selector? windowsSelector;
		static Selector WindowsSelector =>
			windowsSelector ??= new Selector("windows");

		static Selector? uiWindowsSelector;
		static Selector UIWindowsSelector =>
			uiWindowsSelector ??= new Selector("uiWindows");

		Selector? setFrameSelector;
		Selector SetFrameSelector =>
			setFrameSelector ??= new Selector("setFrame:display:animate:");

		public UINSWindow(NativeHandle handle, UIWindow uiWindow)
		{
			Handle = handle;
			UIWindow = uiWindow;
		}

		protected NativeHandle Handle { get; }

		public UIWindow UIWindow { get; }

		public void SetFrame(CGRect frame, bool display = true, bool animate = true)
		{
			var screenHeight = UIWindow.Screen.Bounds.Height;
			frame = new CGRect(
				frame.X / 1.3,
				screenHeight - (frame.Y / 1.3),
				frame.Width / 1.3,
				frame.Height / 1.3);

			void_objc_msgSend_CGRect_bool_bool(Handle.Handle, SetFrameSelector.Handle, frame, display, animate);
		}

		internal static UINSWindow? From(UIWindow uiWindow)
		{
			var nsapp = Runtime.GetNSObject(NSApplicationHandle);
			if (nsapp is null)
				return null;

			var sharedApp = nsapp.PerformSelector(SharedApplicationSelector);
			var windows = sharedApp.PerformSelector(WindowsSelector) as NSArray;

			for (nuint i = 0; i < windows!.Count; i++)
			{
				var nswin = windows.GetItem<NSObject>(i);

				var uiwindows = nswin.PerformSelector(UIWindowsSelector) as NSArray;

				for (nuint j = 0; j < uiwindows!.Count; j++)
				{
					var uiwin = uiwindows.GetItem<UIWindow>(j);

					if (uiwin.Handle == uiWindow.Handle)
						return new UINSWindow(nswin.Handle, uiWindow);
				}
			}

			return null;
		}
	}
}
