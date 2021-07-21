using System;
using System.Runtime.InteropServices;
using TWebView = Tizen.WebView.WebView;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
    public static class WebViewExtension
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

#pragma warning disable IDE0060 // Remove unused parameter
        public static bool SetInterceptRequestResponse(this TWebView webView, IntPtr request, string header, string body, uint length)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return ewk_intercept_request_response_set(request, header, body, length);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static bool IgnoreInterceptRequest(this TWebView webView, IntPtr request)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            return ewk_intercept_request_ignore(request);
        }

#pragma warning disable IDE0060 // Remove unused parameter
		public static string GetInterceptRequestUrl(this TWebView webView, IntPtr request)
#pragma warning restore IDE0060 // Remove unused parameter
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
		internal static extern bool ewk_intercept_request_ignore(IntPtr request);

#pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
		[DllImport(ChromiumEwk)]
#pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
		internal static extern bool ewk_intercept_request_response_set(IntPtr request, string header, string body, uint length);
	}
}
