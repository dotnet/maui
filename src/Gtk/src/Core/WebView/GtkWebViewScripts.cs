using System;
using System.Text.Json;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Web
{
	/// <summary>
	/// The JavaScript and the message framing shared by the GTK web handlers. Pure functions: no GTK or WebKit calls.
	/// </summary>
	internal static class GtkWebViewScripts
	{
		/// <summary>The script message handler name HybridWebView's JavaScript posts to (<c>window.webkit.messageHandlers.webwindowinterop</c>).</summary>
		internal const string HybridMessageHandlerName = "webwindowinterop";

		/// <summary>The scheme of the app origin (<c>app://</c>): WebKit does not let an app serve <c>https</c>.</summary>
		internal const string AppScheme = "app";

		/// <summary>
		/// The script that delivers a raw message from .NET to a HybridWebView page: <c>window.external.receiveMessage("...")</c>, which
		/// HybridWebView.js installs when <c>webkit.messageHandlers.webwindowinterop</c> exists.
		/// </summary>
		/// <param name="rawMessage">The message.</param>
		internal static string ReceiveRawMessage(string rawMessage) =>
			"window.external.receiveMessage(" + JsonSerializer.Serialize(rawMessage) + ")";

		/// <summary>
		/// The script HybridWebView evaluates on the platforms that are not Android: the user script is run through <c>eval</c> and its
		/// result JSON-encoded, so the native side always receives a string.
		/// </summary>
		/// <param name="script">The user script.</param>
		internal static string WrapHybridEvaluate(string script) =>
			"try{JSON.stringify(eval('" + WebViewHelper.EscapeJsString(script) + "'))}catch(e){'null'};";

		/// <summary>
		/// Undoes <see cref="WrapHybridEvaluate"/> on its result: <c>"null"</c> means null (the script failed or returned nothing) and
		/// the quotes <c>JSON.stringify</c> put around a string are removed.
		/// </summary>
		/// <param name="result">The raw result.</param>
		internal static string? UnwrapHybridResult(string? result)
		{
			if (result == "null")
				return null;

			return result?.Trim('"');
		}

		/// <summary>
		/// Turns the value a script evaluated to into the string handed back to MAUI: strings as they are, other values as JSON
		/// (the shape the Android WebView gives), and <see langword="null"/> for <c>undefined</c> and <c>null</c>.
		/// </summary>
		/// <param name="isUndefinedOrNull">Whether the value is <c>undefined</c> or <c>null</c>.</param>
		/// <param name="isString">Whether the value is a string.</param>
		/// <param name="toString">The string conversion of the value.</param>
		/// <param name="toJson">The JSON conversion of the value.</param>
		internal static string? ConvertResult(bool isUndefinedOrNull, bool isString, Func<string?> toString, Func<string?> toJson)
		{
			if (isUndefinedOrNull)
				return null;

			return isString ? toString() : toJson() ?? toString();
		}

		/// <summary>Converts a JavaScriptCore value (see <see cref="ConvertResult(bool, bool, Func{string?}, Func{string?})"/>).</summary>
		/// <param name="value">The evaluation result, or <see langword="null"/>.</param>
		internal static string? ConvertResult(JavaScriptCore.Value? value) =>
			value is null
				? null
				: ConvertResult(value.IsUndefined || value.IsNull, value.IsString, value.ToString, () => value.ToJson(0));

		/// <summary>
		/// Maps WebKit's navigation type to MAUI's navigation event. A pending event the app asked for (<c>GoBack</c>, <c>Reload</c>...) wins over
		/// the WebKit type, because WebKit reports a programmatic load as <c>Other</c>.
		/// </summary>
		/// <param name="type">The WebKit navigation type.</param>
		/// <param name="pending">The event the app requested, or <see cref="WebNavigationEvent.NewPage"/>.</param>
		internal static WebNavigationEvent ResolveNavigationEvent(WebKit.NavigationType type, WebNavigationEvent pending)
		{
			if (pending != WebNavigationEvent.NewPage)
				return pending;

			return type switch
			{
				WebKit.NavigationType.BackForward => WebNavigationEvent.Back,
				WebKit.NavigationType.Reload => WebNavigationEvent.Refresh,
				_ => WebNavigationEvent.NewPage,
			};
		}
	}
}
