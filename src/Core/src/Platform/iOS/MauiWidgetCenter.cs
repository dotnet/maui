#if IOS || MACCATALYST
using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides a C# wrapper around WidgetKit's WidgetCenter for reloading widget timelines
	/// and sharing data with widget extensions via App Groups.
	/// </summary>
	/// <remarks>
	/// WidgetKit is a Swift-only framework with no stable ObjC class names.
	/// This class uses a bundled Swift helper (MauiWidgetHelper) for WidgetCenter calls.
	/// The helper framework must be embedded in the app's Frameworks/ directory.
	/// </remarks>
	[System.Runtime.Versioning.SupportedOSPlatform("ios14.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst14.0")]
	public static class MauiWidgetCenter
	{
		static IntPtr _helperClass;
		static IntPtr _reloadTimelinesSelector;
		static IntPtr _reloadAllTimelinesSelector;
		static bool _initialized;

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libSystem.dylib")]
		static extern IntPtr dlopen(string path, int mode);

		static bool EnsureInitialized()
		{
			if (_initialized)
				return _helperClass != IntPtr.Zero;

			_initialized = true;

			var appBundle = NSBundle.MainBundle.BundlePath;
			var helperPath = $"{appBundle}/Frameworks/MauiWidgetHelper.framework/MauiWidgetHelper";
			var handle = dlopen(helperPath, 1);

			if (handle == IntPtr.Zero)
				return false;

			_helperClass = Class.GetHandle("MauiWidgetHelper");
			if (_helperClass == IntPtr.Zero)
				return false;

			_reloadTimelinesSelector = Selector.GetHandle("reloadTimelines:");
			_reloadAllTimelinesSelector = Selector.GetHandle("reloadAllTimelines");
			return true;
		}

		/// <summary>
		/// Reloads the timelines for all configured widgets of the specified kind.
		/// </summary>
		/// <param name="kind">The widget kind identifier (matches the <c>kind</c> property in your Swift Widget struct).</param>
		public static void ReloadTimelines(string kind)
		{
			if (!EnsureInitialized())
				return;

			using var nsKind = new NSString(kind);
			objc_msgSend_void_IntPtr(_helperClass, _reloadTimelinesSelector, nsKind.Handle);
		}

		/// <summary>
		/// Reloads the timelines for all configured widgets in the app.
		/// </summary>
		public static void ReloadAllTimelines()
		{
			if (!EnsureInitialized())
				return;

			objc_msgSend_void(_helperClass, _reloadAllTimelinesSelector);
		}

		/// <summary>
		/// Gets the shared <see cref="NSUserDefaults"/> for the specified App Group,
		/// used to share data between the main app and widget extensions.
		/// </summary>
		/// <param name="appGroupId">The App Group identifier (e.g., "group.com.mycompany.myapp").</param>
		/// <returns>An <see cref="NSUserDefaults"/> instance for the App Group, or <c>null</c> if the group is invalid.</returns>
		public static NSUserDefaults? GetSharedDefaults(string appGroupId)
		{
			return new NSUserDefaults(appGroupId, NSUserDefaultsType.SuiteName);
		}
	}
}
#endif
