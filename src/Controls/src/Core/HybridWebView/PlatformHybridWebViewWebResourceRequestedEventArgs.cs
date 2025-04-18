#pragma warning disable RS0016 // Add public types and members to the declared API
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls;

public class PlatformHybridWebViewWebResourceRequestedEventArgs
{
#if WINDOWS
	internal PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::Microsoft.Web.WebView2.Core.CoreWebView2 sender,
		global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs eventArgs)
	{
		Sender = sender;
		RequestEventArgs = eventArgs;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.RequestEventArgs)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2 Sender { get; }

	/// <summary>
	/// 
	/// </summary>
	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs RequestEventArgs { get; }

	public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequest Request => RequestEventArgs.Request;

	internal string? GetRequestUri() => Request.Uri;

	internal string? GetRequestMethod() => Request.Method;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => new WrappedHeadersDictionary(Request.Headers);

	class WrappedHeadersDictionary : IReadOnlyDictionary<string, string>
	{
		private global::Microsoft.Web.WebView2.Core.CoreWebView2HttpRequestHeaders _headers;

		public WrappedHeadersDictionary(global::Microsoft.Web.WebView2.Core.CoreWebView2HttpRequestHeaders headers)
		{
			_headers = headers;
		}

		public string this[string key] =>
			_headers.Contains(key)
				? _headers.GetHeader(key)
				: throw new KeyNotFoundException($"The key '{key}' was not found.");

		public IEnumerable<string> Keys =>
			_headers.Select(header => header.Key);

		public IEnumerable<string> Values =>
			_headers.Select(header => header.Value);

		public int Count => _headers.Count();

		public bool ContainsKey(string key) => _headers.Contains(key);
		
		public bool TryGetValue(string key, out string value)
		{
			if (_headers.Contains(key))
			{
				value = _headers.GetHeader(key);
				return true;
			}
			value = string.Empty;
			return false;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _headers.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

#elif IOS || MACCATALYST

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::WebKit.WKWebView sender,
		global::WebKit.IWKUrlSchemeTask urlSchemeTask)
	{
		Sender = sender;
		UrlSchemeTask = urlSchemeTask;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.UrlSchemeTask)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::WebKit.WKWebView Sender { get; }

	public global::WebKit.IWKUrlSchemeTask UrlSchemeTask { get; }

	public Foundation.NSUrlRequest Request => UrlSchemeTask.Request;

	internal string? GetRequestUri() => Request.Url?.AbsoluteString;

	internal string? GetRequestMethod() => Request.HttpMethod;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => new WrappedHeadersDictionary(Request.Headers);

	class WrappedHeadersDictionary : IReadOnlyDictionary<string, string>
	{
		Foundation.NSDictionary _headers;

		public WrappedHeadersDictionary(Foundation.NSDictionary headers)
		{
			_headers = headers;
		}

		public string this[string key] =>
			 TryGetValue(key, out var value)
				? value
				: throw new KeyNotFoundException($"The key '{key}' was not found.");

		public IEnumerable<string> Keys => _headers.Keys.Select(k => k.ToString());

		public IEnumerable<string> Values => _headers.Values.Select(v => v.ToString());

		public int Count => (int)_headers.Count;

		public bool ContainsKey(string key)
		{
			using var nskey = new Foundation.NSString(key);
			return _headers.ContainsKey(nskey);
		}

		public bool TryGetValue(string key, out string value)
		{
			using var nsKey = new Foundation.NSString(key);
			if (_headers.ContainsKey(nsKey))
			{
				value = _headers[nsKey].ToString();
				return true;
			}
			value = string.Empty;
			return false;
		}

		public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
		{
			foreach (var pair in _headers)
			{
				yield return new KeyValuePair<string, string>(pair.Key.ToString(), pair.Value.ToString());
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

#elif ANDROID

	Action<global::Android.Webkit.WebResourceResponse?> _setResponse;
	private global::Android.Webkit.WebResourceResponse? _response;

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(
		global::Android.Webkit.WebView sender,
		global::Android.Webkit.IWebResourceRequest request,
		Action<global::Android.Webkit.WebResourceResponse?> setResponse)
	{
		Sender = sender;
		Request = request;
		_setResponse = setResponse;
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
		: this(args.Sender, args.Request, (response) => args.Response = response)
	{
	}

	/// <summary>
	/// Gets the native view attached to the event.
	/// </summary>
	public global::Android.Webkit.WebView Sender { get; }

	/// <summary>
	/// 
	/// </summary>
	public global::Android.Webkit.IWebResourceRequest Request { get; }

	public global::Android.Webkit.WebResourceResponse? Response
	{
		get => _response;
		set
		{
			_response = value;
			_setResponse(value);
		}
	}

	internal string? GetRequestUri() => Request.Url?.ToString();

	internal string? GetRequestMethod() => Request.Method;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => Request.RequestHeaders?.AsReadOnly();

#else

	internal PlatformHybridWebViewWebResourceRequestedEventArgs()
	{
	}

	internal PlatformHybridWebViewWebResourceRequestedEventArgs(WebResourceRequestedEventArgs args)
	{
	}

#pragma warning disable CA1822 // Mark members as static
	internal string? GetRequestUri() => null;

	internal string? GetRequestMethod() => null;

	internal IReadOnlyDictionary<string, string>? GetRequestHeaders() => null;
#pragma warning restore CA1822 // Mark members as static

#endif
}
