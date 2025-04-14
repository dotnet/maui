﻿#pragma warning disable RS0016 // Add public types and members to the declared API
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	public interface IHybridWebView : IView
	{
		/// <summary>
		/// Specifies the file within the <see cref="HybridRoot"/> that should be served as the default file. The
		/// default value is <c>index.html</c>.
		/// </summary>
		string? DefaultFile { get; }

		/// <summary>
		///  The path within the app's "Raw" asset resources that contain the web app's contents. For example, if the
		///  files are located in <c>[ProjectFolder]/Resources/Raw/hybrid_root</c>, then set this property to "hybrid_root".
		///  The default value is <c>wwwroot</c>, which maps to <c>[ProjectFolder]/Resources/Raw/wwwroot</c>.
		/// </summary>
		string? HybridRoot { get; }

		/// <summary>
		/// For internal use only.
		/// </summary>
		object? InvokeJavaScriptTarget { get; set; }

		/// <summary>
		///  Sets the object that will be the target of JavaScript calls from the web view. The public methods on this object
		///  are callable from JavaScript using the <c>window.HybridWebView.InvokeDotNet</c> method.
		/// </summary>
		/// <typeparam name="T">The type that contains methods callable from JavaScript.</typeparam>
		/// <param name="target">An instance of type <typeparamref name="T"/> that will be used to call methods on.</param>
		void SetInvokeJavaScriptTarget<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(T target) where T : class;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
		Type? InvokeJavaScriptType { get; set; }

		void RawMessageReceived(string rawMessage);

		void SendRawMessage(string rawMessage);

		/// <summary>
		/// Runs the JavaScript code provided in the <paramref name="script"/> parameter and returns the result as a string.
		/// </summary>
		/// <param name="script">The JavaScript code to run.</param>
		/// <returns>The return value (if any) of running the script.</returns>
		Task<string?> EvaluateJavaScriptAsync(string script);

		/// <summary>
		/// Invokes a JavaScript method named <paramref name="methodName"/> and optionally passes in the parameter values specified
		/// by <paramref name="paramValues"/> by JSON-encoding each one.
		/// </summary>
		/// <typeparam name="TReturnType">The type of the return value.</typeparam>
		/// <param name="methodName">The name of the JavaScript method to invoke.</param>
		/// <param name="returnTypeJsonTypeInfo">Metadata about deserializing the type of the return value specified by <typeparamref name="TReturnType"/>.</param>
		/// <param name="paramValues">Optional array of objects to be passed to the JavaScript method by JSON-encoding each one.</param>
		/// <param name="paramJsonTypeInfos">Optional array of metadata about serializing the types of the parameters specified by <paramref name="paramValues"/>.</param>
		/// <returns>An object of type <typeparamref name="TReturnType"/> containing the return value of the called method.</returns>
		Task<TReturnType?> InvokeJavaScriptAsync<TReturnType>(
			string methodName,
			JsonTypeInfo<TReturnType> returnTypeJsonTypeInfo,
			object?[]? paramValues = null,
			JsonTypeInfo?[]? paramJsonTypeInfos = null);

#if NETSTANDARD
		void OnAboutToSendRequest(HybridWebViewAboutToSendRequestEventArgs args);
#else
		void OnAboutToSendRequest(HybridWebViewAboutToSendRequestEventArgs args) { }
#endif
	}

	public class HybridWebViewAboutToSendRequestEventArgs
	{
#if WINDOWS
		internal HybridWebViewAboutToSendRequestEventArgs(HybridWebViewPlatformAboutToSendRequestEventArgs platformArgs)
			: this(platformArgs.Request.Uri)
		{
			PlatformArgs = platformArgs;
		}
#elif IOS || MACCATALYST
		internal HybridWebViewAboutToSendRequestEventArgs(HybridWebViewPlatformAboutToSendRequestEventArgs platformArgs)
			: this(platformArgs.Request.Url!.AbsoluteString!)
		{
			PlatformArgs = platformArgs;
		}
#elif ANDROID
		internal HybridWebViewAboutToSendRequestEventArgs(HybridWebViewPlatformAboutToSendRequestEventArgs platformArgs)
			: this(platformArgs.Request.Url!.ToString()!)
		{
			PlatformArgs = platformArgs;
		}
#endif

		public HybridWebViewAboutToSendRequestEventArgs(string uri)
			: this(new Uri(uri))
		{
		}

		public HybridWebViewAboutToSendRequestEventArgs(Uri uri)
		{
			RequestUri = uri;
		}

		public HybridWebViewPlatformAboutToSendRequestEventArgs? PlatformArgs { get; }

		public Uri RequestUri { get; }

		public bool Handled { get; set; }
	}

	public class HybridWebViewPlatformAboutToSendRequestEventArgs
	{
#if WINDOWS
		internal HybridWebViewPlatformAboutToSendRequestEventArgs(
			global::Microsoft.Web.WebView2.Core.CoreWebView2 sender,
			global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs eventArgs)
		{
			Sender = sender;
			RequestEventArgs = eventArgs;
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

		public global::Microsoft.Web.WebView2.Core.CoreWebView2WebResourceResponse? Response
		{
			get => RequestEventArgs.Response;
			set => RequestEventArgs.Response = value;
		}

#elif IOS || MACCATALYST

		public HybridWebViewPlatformAboutToSendRequestEventArgs(
			global::WebKit.WKWebView sender,
			global::WebKit.IWKUrlSchemeTask urlSchemeTask)
		{
			Sender = sender;
			UrlSchemeTask = urlSchemeTask;
		}

		/// <summary>
		/// Gets the native view attached to the event.
		/// </summary>
		public global::WebKit.WKWebView Sender { get; }

		public global::WebKit.IWKUrlSchemeTask UrlSchemeTask { get; }

		public global::Foundation.NSUrlRequest Request => UrlSchemeTask.Request;

#elif ANDROID

		public HybridWebViewPlatformAboutToSendRequestEventArgs(
			global::Android.Webkit.WebView sender,
			global::Android.Webkit.IWebResourceRequest request)
		{
			Sender = sender;
			Request = request;
		}

		/// <summary>
		/// Gets the native view attached to the event.
		/// </summary>
		public global::Android.Webkit.WebView Sender { get; }

		/// <summary>
		/// 
		/// </summary>
		public global::Android.Webkit.IWebResourceRequest Request { get; }

		public global::Android.Webkit.WebResourceResponse? Response { get; set; }

#else

		internal HybridWebViewPlatformAboutToSendRequestEventArgs()
		{
		}

#endif
	}
}

