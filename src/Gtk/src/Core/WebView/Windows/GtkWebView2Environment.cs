using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// Availability of the WebView2 Runtime and the one <see cref="CoreWebView2Environment"/> of the process, the engine behind the GTK web
	/// handlers on Windows.
	/// </summary>
	/// <remarks>
	/// <para>Set <c>MAUI_GTK_DISABLE_WEBVIEW2=1</c> to force the placeholder (tests, or machines where the runtime is installed but unusable).
	/// The user data folder is <c>WEBVIEW2_USER_DATA_FOLDER</c> when that variable is set (WebView2 reads it itself), otherwise
	/// <c>%LOCALAPPDATA%\&lt;process name&gt;\WebView2</c>.</para>
	/// <para>Seam for the HybridWebView and BlazorWebView tracks: <see cref="GetAsync"/> is the environment every web view of the
	/// process shares (WebView2 requires the same options and user data folder for the web views of one process).</para>
	/// </remarks>
	internal static class GtkWebView2Environment
	{
		/// <summary>The environment variable that forces the placeholder even when the runtime is installed.</summary>
		internal const string DisableVariable = "MAUI_GTK_DISABLE_WEBVIEW2";

		/// <summary>Where the user can download the WebView2 Runtime.</summary>
		internal const string RuntimeDownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

		static readonly object s_gate = new();
		static bool s_probed;
		static string? s_reason;
		static string? s_version;
		static Task<CoreWebView2Environment>? s_environment;

		/// <summary>Test seam: replaces the probe of the installed runtime version (<see langword="null"/> restores the real one).</summary>
		internal static Func<string?>? ProbeOverride { get; set; }

		/// <summary>Whether the WebView2 Runtime can be used in this process.</summary>
		internal static bool IsAvailable
		{
			get
			{
				Probe();
				return s_reason is null;
			}
		}

		/// <summary>Why <see cref="IsAvailable"/> is <see langword="false"/>, or <see langword="null"/> when it is available.</summary>
		internal static string? UnavailableReason
		{
			get
			{
				Probe();
				return s_reason;
			}
		}

		/// <summary>The installed runtime version, or <see langword="null"/> when there is none.</summary>
		internal static string? RuntimeVersion
		{
			get
			{
				Probe();
				return s_version;
			}
		}

		/// <summary>Forgets the probe result (tests).</summary>
		internal static void ResetProbe()
		{
			lock (s_gate)
			{
				s_probed = false;
				s_reason = null;
				s_version = null;
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
					OperatingSystem.IsWindows(),
					Environment.GetEnvironmentVariable(DisableVariable),
					ProbeOverride ?? ReadRuntimeVersion,
					out s_version);
			}
		}

		/// <summary>The pure decision behind <see cref="UnavailableReason"/>: <see langword="null"/> means available.</summary>
		internal static string? Evaluate(bool isWindows, string? disableValue, Func<string?> probeVersion, out string? version)
		{
			version = null;

			if (!isWindows)
				return "WebView2 is only available on Windows.";

			if (!string.IsNullOrEmpty(disableValue) && disableValue != "0")
				return DisableVariable + " is set.";

			try
			{
				version = probeVersion();
				if (string.IsNullOrEmpty(version))
					version = null;
			}
			catch (Exception e) when (e is DllNotFoundException or BadImageFormatException or EntryPointNotFoundException)
			{
				return "WebView2 Runtime not installed (" + RuntimeDownloadUrl + "): its loader could not be loaded (" + e.Message + ").";
			}
			catch (Exception)
			{
				version = null;
			}

			return string.IsNullOrEmpty(version)
				? "WebView2 Runtime not installed (" + RuntimeDownloadUrl + ")"
				: null;
		}

		static string? ReadRuntimeVersion() => CoreWebView2Environment.GetAvailableBrowserVersionString();

		/// <summary>The default user data folder (see the type remarks).</summary>
		internal static string? DefaultUserDataFolder(string? variable, string localAppData, string processName) =>
			string.IsNullOrEmpty(variable) ? Path.Combine(localAppData, processName, "WebView2") : null;

		/// <summary>
		/// The shared environment, created on first use. Call it on the GTK thread: WebView2 completes asynchronously through the thread's
		/// message pump, which GTK's main loop runs, and the continuation runs on that thread.
		/// </summary>
		internal static Task<CoreWebView2Environment> GetAsync()
		{
			lock (s_gate)
			{
				if (s_environment is { IsFaulted: false, IsCanceled: false })
					return s_environment;

				var folder = DefaultUserDataFolder(
					Environment.GetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER"),
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					Path.GetFileNameWithoutExtension(Environment.ProcessPath) is { Length: > 0 } name ? name : "MauiGtk");

				return s_environment = CoreWebView2Environment.CreateAsync(null, folder);
			}
		}
	}
}
