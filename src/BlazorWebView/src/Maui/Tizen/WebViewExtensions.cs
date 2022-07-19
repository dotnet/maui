using System;
using System.Runtime.InteropServices;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	/// <summary>
	/// WebViewExtension
	/// </summary>
	internal static class WebViewExtensions
	{
		public const string ChromiumEwk = "libchromium-ewk.so";

		public static void SetInterceptRequestCallback(this TWebView webView, InterceptRequestCallback callback)
		{
			var context = webView.GetContext();
			var handleField = context.GetType().GetField("_handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var contextHandle = (IntPtr?)handleField?.GetValue(context);
			if (contextHandle != null)
				ewk_context_intercept_request_callback_set(contextHandle.Value, callback, IntPtr.Zero);
		}

		public static void SetInspectorStart(this TWebView webView, uint port)
		{
			var context = webView.GetContext();
			var handleField = context.GetType().GetField("_handle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var contextHandle = (IntPtr?)handleField?.GetValue(context);
			if (contextHandle != null)
				ewk_context_inspector_server_start(contextHandle.Value, port);
		}

		public static bool SetInterceptRequestResponse(this TWebView webView, IntPtr request, string header, byte[] body, uint length)
		{
			return ewk_intercept_request_response_set(request, header, body, length);
		}

		public static bool IgnoreInterceptRequest(this TWebView webView, IntPtr request)
		{
			return ewk_intercept_request_ignore(request);
		}

		public static string GetInterceptRequestUrl(this TWebView webView, IntPtr request)
		{
			return Marshal.PtrToStringAnsi(_ewk_intercept_request_url_get(request)) ?? string.Empty;
		}

		[DllImport(ChromiumEwk)]
		internal static extern IntPtr ewk_view_context_get(IntPtr obj);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void InterceptRequestCallback(IntPtr context, IntPtr request, IntPtr userData);

		[DllImport(ChromiumEwk)]
		internal static extern void ewk_context_intercept_request_callback_set(IntPtr context, InterceptRequestCallback callback, IntPtr userData);

		[DllImport(ChromiumEwk, EntryPoint = "ewk_intercept_request_url_get")]
		internal static extern IntPtr _ewk_intercept_request_url_get(IntPtr request);

		[DllImport(ChromiumEwk, EntryPoint = "ewk_intercept_request_http_method_get")]
		internal static extern IntPtr _ewk_intercept_request_http_method_get(IntPtr request);

		internal static string ewk_intercept_request_http_method_get(IntPtr request)
		{
			return Marshal.PtrToStringAnsi(_ewk_intercept_request_http_method_get(request)) ?? string.Empty;
		}

		[DllImport(ChromiumEwk)]
		public static extern uint ewk_context_inspector_server_start(IntPtr context, uint port);

		[DllImport(ChromiumEwk)]
		internal static extern bool ewk_intercept_request_ignore(IntPtr request);

		[DllImport(ChromiumEwk)]
		internal static extern bool ewk_intercept_request_response_set(IntPtr request, string header, string body, uint length);

		[DllImport(ChromiumEwk)]
		internal static extern bool ewk_intercept_request_response_set(IntPtr request, string header, byte[] body, uint length);
	}
}
