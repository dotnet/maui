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
		const int RTLD_LAZY = 1;

		static IntPtr _helperClass;
		static IntPtr _reloadTimelinesSelector;
		static IntPtr _reloadAllTimelinesSelector;
		static bool _initialized;
		static readonly object _initLock = new object();

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

			lock (_initLock)
			{
				if (_initialized)
					return _helperClass != IntPtr.Zero;

				var appBundle = NSBundle.MainBundle.BundlePath;
				var helperPath = $"{appBundle}/Frameworks/MauiWidgetHelper.framework/MauiWidgetHelper";
				var handle = dlopen(helperPath, RTLD_LAZY);

				if (handle == IntPtr.Zero)
				{
					_initialized = true;
					return false;
				}

				_helperClass = Class.GetHandle("MauiWidgetHelper");
				if (_helperClass == IntPtr.Zero)
				{
					_initialized = true;
					return false;
				}

				_reloadTimelinesSelector = Selector.GetHandle("reloadTimelines:");
				_reloadAllTimelinesSelector = Selector.GetHandle("reloadAllTimelines");
				_initialized = true;
				return true;
			}
		}

		/// <summary>
		/// Reloads the timelines for all configured widgets of the specified kind.
		/// </summary>
		/// <param name="kind">The widget kind identifier (matches the <c>kind</c> property in your Swift Widget struct).</param>
		/// <exception cref="ArgumentNullException"><paramref name="kind"/> is <c>null</c>.</exception>
		public static void ReloadTimelines(string kind)
		{
			ArgumentNullException.ThrowIfNull(kind);

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
		/// Gets the URL for the App Group shared container directory.
		/// Use this to read/write JSON files shared between the app and widget extension.
		/// </summary>
		/// <remarks>
		/// File-based I/O via the shared container is more reliable than <see cref="GetSharedDefaults"/>
		/// for cross-process communication, especially on the iOS Simulator with ad-hoc signing.
		/// </remarks>
		/// <param name="appGroupId">The App Group identifier (e.g., "group.com.mycompany.myapp").</param>
		/// <returns>An <see cref="NSUrl"/> for the container directory, or <c>null</c> if unavailable.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="appGroupId"/> is <c>null</c>.</exception>
		public static NSUrl? GetSharedContainerUrl(string appGroupId)
		{
			ArgumentNullException.ThrowIfNull(appGroupId);
			return NSFileManager.DefaultManager.GetContainerUrl(appGroupId);
		}

		/// <summary>
		/// Gets the shared <see cref="NSUserDefaults"/> for the specified App Group.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <c>NSUserDefaults(suiteName:)</c> can resolve to different backing plist files for the
		/// app vs. the widget extension process, especially on the iOS Simulator or with ad-hoc signing.
		/// For reliable cross-process data sharing, prefer <see cref="GetSharedContainerUrl"/> with
		/// file-based JSON I/O instead.
		/// </para>
		/// </remarks>
		/// <param name="appGroupId">The App Group identifier (e.g., "group.com.mycompany.myapp").</param>
		/// <returns>An <see cref="NSUserDefaults"/> instance for the App Group.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="appGroupId"/> is <c>null</c>.</exception>
		public static NSUserDefaults GetSharedDefaults(string appGroupId)
		{
			ArgumentNullException.ThrowIfNull(appGroupId);
			return new NSUserDefaults(appGroupId, NSUserDefaultsType.SuiteName);
		}
	}
}
#endif
