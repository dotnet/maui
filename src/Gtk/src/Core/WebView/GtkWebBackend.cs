using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Gtk.Platform;
using GtkLabel = Gtk.Label;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>The browser engine a GTK web handler runs on.</summary>
	internal enum GtkWebBackendKind
	{
		/// <summary>No engine is usable: the handlers create a placeholder label.</summary>
		None,

		/// <summary>WebKitGTK 6 (Linux).</summary>
		WebKitGtk,

		/// <summary>WebView2, hosted as a child window of the GTK toplevel (Windows).</summary>
		WebView2,
	}

	/// <summary>
	/// Chooses the engine of the web handlers at run time and creates their platform view: WebKitGTK on Linux, WebView2 on Windows, a
	/// placeholder label that says why there is none otherwise.
	/// </summary>
	/// <remarks>
	/// <see cref="CreatePlatformView"/> / <see cref="CreateEngine"/> are the seam the handlers use: the WebView handler goes through both; the
	/// HybridWebView and BlazorWebView handlers go through <see cref="CreatePlatformView"/> too (reaching engine-specific features through <see cref="MauiGtkWebView2Host"/>).
	/// </remarks>
	internal static class GtkWebBackend
	{
		/// <summary>The pure engine choice: Linux prefers WebKitGTK, Windows WebView2, and an unavailable engine is never chosen.</summary>
		internal static GtkWebBackendKind Select(bool isLinux, bool isWindows, bool webKitAvailable, bool webView2Available)
		{
			if (isLinux && webKitAvailable)
				return GtkWebBackendKind.WebKitGtk;

			if (isWindows && webView2Available)
				return GtkWebBackendKind.WebView2;

			return GtkWebBackendKind.None;
		}

		/// <summary>The engine this process uses.</summary>
		internal static GtkWebBackendKind Current =>
			Select(
				OperatingSystem.IsLinux(),
				OperatingSystem.IsWindows(),
				GtkWebKit.IsAvailable,
				OperatingSystem.IsWindows() && IsWebView2Available());

		/// <summary>Why no engine is usable, or <see langword="null"/> when <see cref="Current"/> is not <see cref="GtkWebBackendKind.None"/>.</summary>
		internal static string? UnavailableReason =>
			Current != GtkWebBackendKind.None
				? null
				: OperatingSystem.IsWindows() ? WebView2UnavailableReason() : GtkWebKit.UnavailableReason;

		/// <summary>The text of the placeholder label of a handler whose engine is unusable.</summary>
		internal static string PlaceholderText(string controlName, string? reason) =>
			controlName + " is unavailable: " + (reason ?? "no web engine could be created.");

		/// <summary>Creates the platform view of a web handler: the engine's widget, or a label that says why there is none.</summary>
		internal static global::Gtk.Widget CreatePlatformView(string controlName)
		{
			switch (Current)
			{
				case GtkWebBackendKind.WebKitGtk:
					if (GtkWebKit.TryCreateWebView() is { } view)
						return view;

					break;

				case GtkWebBackendKind.WebView2:
					return CreateWebView2Host();
			}

			return new GtkLabel
			{
				Text = PlaceholderText(controlName, UnavailableReason),
				Wrap = true,
				Selectable = true,
			};
		}

		/// <summary>
		/// The engine of a platform view made by <see cref="CreatePlatformView"/>, or <see langword="null"/> for a placeholder label. The engine
		/// is the handler's to dispose when it disconnects.
		/// </summary>
		internal static IGtkWebEngine? CreateEngine(global::Gtk.Widget platformView)
		{
			if (platformView is global::WebKit.WebView view)
				return new GtkWebKitEngine(view);

			return OperatingSystem.IsWindows() ? GetWebView2Engine(platformView) : null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static bool IsWebView2Available() => GtkWebView2Environment.IsAvailable;

		[MethodImpl(MethodImplOptions.NoInlining)]
		static string? WebView2UnavailableReason() => GtkWebView2Environment.UnavailableReason;

		[MethodImpl(MethodImplOptions.NoInlining)]
		static global::Gtk.Widget CreateWebView2Host()
		{
			GtkRuntime.EnsureInitialized();
			return new MauiGtkWebView2Host();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static IGtkWebEngine? GetWebView2Engine(global::Gtk.Widget platformView) =>
			(platformView as MauiGtkWebView2Host)?.Engine;
	}
}
