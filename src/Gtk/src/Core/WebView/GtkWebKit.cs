using System;
using System.Runtime.InteropServices;
using Microsoft.Maui.Gtk.Platform;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// Availability of WebKitGTK 6 (<c>libwebkitgtk-6.0</c>), the engine behind the WebView, HybridWebView and BlazorWebView handlers.
	/// </summary>
	/// <remarks>
	/// WebKitGTK exists only on Linux (gvsbuild ships no WebKit for Windows). When it cannot be loaded the handlers create a
	/// placeholder label that says why instead of throwing, so an app with a WebView still starts. Set
	/// <c>MAUI_GTK_DISABLE_WEBKIT=1</c> to force the placeholder (tests, or machines where WebKit is installed but unusable).
	/// </remarks>
	public static class GtkWebKit
	{
		/// <summary>The environment variable that forces the placeholder even when WebKitGTK is installed.</summary>
		public const string DisableVariable = "MAUI_GTK_DISABLE_WEBKIT";

		static readonly string[] s_libraryNames = { "libwebkitgtk-6.0.so.4", "libwebkitgtk-6.0.so" };
		static readonly object s_gate = new();
		static bool s_probed;
		static bool s_available;
		static string? s_reason;

		/// <summary>Whether WebKitGTK 6 can be loaded in this process.</summary>
		public static bool IsAvailable
		{
			get
			{
				Probe();
				return s_available;
			}
		}

		/// <summary>Why <see cref="IsAvailable"/> is <see langword="false"/>, or <see langword="null"/> when WebKitGTK is available.</summary>
		public static string? UnavailableReason
		{
			get
			{
				Probe();
				return s_reason;
			}
		}

		static void Probe()
		{
			lock (s_gate)
			{
				if (s_probed)
					return;

				s_probed = true;
				s_reason = Evaluate(
					OperatingSystem.IsLinux(),
					Environment.GetEnvironmentVariable(DisableVariable),
					TryLoadNativeLibrary);
				s_available = s_reason is null;
			}
		}

		/// <summary>The pure decision behind <see cref="UnavailableReason"/>: <see langword="null"/> means available.</summary>
		internal static string? Evaluate(bool isLinux, string? disableValue, Func<bool> tryLoadLibrary)
		{
			if (!isLinux)
				return "WebKitGTK (libwebkitgtk-6.0) is only available on Linux.";

			if (!string.IsNullOrEmpty(disableValue) && disableValue != "0")
				return DisableVariable + " is set.";

			return tryLoadLibrary()
				? null
				: "libwebkitgtk-6.0 could not be loaded; install the WebKitGTK 6 package (for example gir1.2-webkit-6.0 / webkitgtk-6.0).";
		}

		static bool TryLoadNativeLibrary()
		{
			foreach (var name in s_libraryNames)
			{
				if (NativeLibrary.TryLoad(name, out var handle))
				{
					NativeLibrary.Free(handle);
					return true;
				}
			}

			return false;
		}

		/// <summary>Records that WebKitGTK turned out to be unusable after all (a native symbol or type failed to load).</summary>
		static void MarkUnavailable(string reason)
		{
			lock (s_gate)
			{
				s_probed = true;
				s_available = false;
				s_reason = reason;
			}
		}

		/// <summary>Creates a <c>WebKit.WebView</c>, or returns <see langword="null"/> (and records why) when WebKitGTK is unusable.</summary>
		internal static global::WebKit.WebView? TryCreateWebView()
		{
			if (!IsAvailable)
				return null;

			try
			{
				GtkRuntime.EnsureInitialized();
				return new global::WebKit.WebView();
			}
			catch (Exception e) when (e is DllNotFoundException or EntryPointNotFoundException or TypeInitializationException or BadImageFormatException)
			{
				MarkUnavailable("WebKitGTK failed to initialise: " + (e.InnerException?.Message ?? e.Message));
				return null;
			}
		}

		/// <summary>Creates the platform view of a web handler: a real web view, or a label that says why there is none.</summary>
		internal static global::Gtk.Widget CreatePlatformView(string controlName)
		{
			var view = TryCreateWebView();
			if (view is not null)
				return view;

			return new GtkLabel
			{
				Text = controlName + " is unavailable: " + (UnavailableReason ?? "WebKitGTK could not be created."),
				Wrap = true,
				Selectable = true,
			};
		}
	}
}
