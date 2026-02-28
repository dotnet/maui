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
	/// WidgetKit is not included in the .NET iOS bindings, so this class uses ObjC runtime
	/// interop to access WidgetCenter APIs directly.
	/// </remarks>
	[System.Runtime.Versioning.SupportedOSPlatform("ios14.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst14.0")]
	public static class MauiWidgetCenter
	{
		static IntPtr _widgetCenterClass;
		static IntPtr _sharedSelector;
		static IntPtr _reloadTimelinesSelector;
		static IntPtr _reloadAllTimelinesSelector;
		static bool _initialized;

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);

		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		static extern void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

		[DllImport("/usr/lib/libSystem.dylib")]
		static extern IntPtr dlopen(string path, int mode);

		static bool EnsureInitialized()
		{
			if (_initialized)
				return _widgetCenterClass != IntPtr.Zero;

			_initialized = true;

			// Load WidgetKit framework
			dlopen("/System/Library/Frameworks/WidgetKit.framework/WidgetKit", 0);

			_widgetCenterClass = Class.GetHandle("WGWidgetCenter");
			if (_widgetCenterClass == IntPtr.Zero)
				return false;

			_sharedSelector = Selector.GetHandle("sharedCenter");
			_reloadTimelinesSelector = Selector.GetHandle("reloadTimelinesOfKind:");
			_reloadAllTimelinesSelector = Selector.GetHandle("reloadAllTimelines");
			return true;
		}

		static IntPtr GetSharedCenter()
		{
			if (!EnsureInitialized())
				return IntPtr.Zero;

			return objc_msgSend_IntPtr(_widgetCenterClass, _sharedSelector);
		}

		/// <summary>
		/// Reloads the timelines for all configured widgets of the specified kind.
		/// </summary>
		/// <param name="kind">The widget kind identifier (matches the <c>kind</c> property in your Swift Widget struct).</param>
		public static void ReloadTimelines(string kind)
		{
			var center = GetSharedCenter();
			if (center == IntPtr.Zero)
				return;

			using var nsKind = new NSString(kind);
			objc_msgSend_void_IntPtr(center, _reloadTimelinesSelector, nsKind.Handle);
		}

		/// <summary>
		/// Reloads the timelines for all configured widgets in the app.
		/// </summary>
		public static void ReloadAllTimelines()
		{
			var center = GetSharedCenter();
			if (center == IntPtr.Zero)
				return;

			objc_msgSend_void(center, _reloadAllTimelinesSelector);
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
