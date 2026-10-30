using System;
using System.Runtime.InteropServices;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// Connects the WebKit signals whose handlers return a <c>gboolean</c> (<c>decide-policy</c>, <c>load-failed</c>) directly with
	/// <c>g_signal_connect_data</c>.
	/// </summary>
	/// <remarks>
	/// The GtkSharp event wrappers of these two signals never invoke their handlers (verified against WebKitGTK 2.52: <c>load-changed</c> and
	/// <c>script-message-received</c> arrive, <c>decide-policy</c> and <c>load-failed</c> do not), so the handlers are connected natively.
	/// Only called when <see cref="GtkWebKit.IsAvailable"/>, that is on Linux.
	/// </remarks>
	internal sealed class GtkWebKitSignals : IDisposable
	{
		const string GObjectLibrary = "libgobject-2.0.so.0";

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int DecidePolicyNative(IntPtr view, IntPtr decision, int decisionType, IntPtr userData);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int LoadFailedNative(IntPtr view, int loadEvent, IntPtr failingUri, IntPtr error, IntPtr userData);

		[DllImport(GObjectLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern ulong g_signal_connect_data(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string detailedSignal, IntPtr handler, IntPtr data, IntPtr destroyData, int flags);

		[DllImport(GObjectLibrary, CallingConvention = CallingConvention.Cdecl)]
		static extern void g_signal_handler_disconnect(IntPtr instance, ulong handlerId);

		readonly IntPtr _view;
		readonly DecidePolicyNative? _decide;
		readonly LoadFailedNative? _failed;
		ulong _decideId;
		ulong _failedId;

		/// <summary>Connects the handlers.</summary>
		/// <param name="view">The web view.</param>
		/// <param name="onDecidePolicy">Handles <c>decide-policy</c>; returns <see langword="true"/> when it made the decision.</param>
		/// <param name="onLoadFailed">Handles <c>load-failed</c> (failing URI); returns <see langword="true"/> to stop WebKit's default handling.</param>
		internal GtkWebKitSignals(global::WebKit.WebView view, Func<IntPtr, WebKit.PolicyDecisionType, bool>? onDecidePolicy, Func<WebKit.LoadEvent, string?, bool>? onLoadFailed)
		{
			_view = view.Handle;

			if (onDecidePolicy is not null)
			{
				_decide = (v, decision, type, data) =>
				{
					try
					{
						return onDecidePolicy(decision, (WebKit.PolicyDecisionType)type) ? 1 : 0;
					}
					catch (Exception)
					{
						// A managed exception must not cross into WebKit.
						return 0;
					}
				};
				_decideId = g_signal_connect_data(_view, "decide-policy", Marshal.GetFunctionPointerForDelegate(_decide), IntPtr.Zero, IntPtr.Zero, 0);
			}

			if (onLoadFailed is not null)
			{
				_failed = (v, loadEvent, failingUri, error, data) =>
				{
					try
					{
						return onLoadFailed((WebKit.LoadEvent)loadEvent, Marshal.PtrToStringUTF8(failingUri)) ? 1 : 0;
					}
					catch (Exception)
					{
						return 0;
					}
				};
				_failedId = g_signal_connect_data(_view, "load-failed", Marshal.GetFunctionPointerForDelegate(_failed), IntPtr.Zero, IntPtr.Zero, 0);
			}
		}

		/// <summary>Disconnects the handlers.</summary>
		public void Dispose()
		{
			if (_decideId != 0)
				g_signal_handler_disconnect(_view, _decideId);
			if (_failedId != 0)
				g_signal_handler_disconnect(_view, _failedId);

			_decideId = 0;
			_failedId = 0;
		}
	}
}
