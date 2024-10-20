﻿using System;
using Android.Content;
using Android.Webkit;
using AWebView = Android.Webkit.WebView;
using AUri = Android.Net.Uri;

namespace Microsoft.Maui.Platform
{
	public class MauiHybridWebView : AWebView, IHybridPlatformWebView
	{
		private readonly WeakReference<HybridWebViewHandler> _handler;
		private static readonly AUri AndroidAppOriginUri = AUri.Parse(HybridWebViewHandler.AppOrigin)!;

		public MauiHybridWebView(HybridWebViewHandler handler, Context context) : base(context)
		{
			ArgumentNullException.ThrowIfNull(handler, nameof(handler));
			_handler = new WeakReference<HybridWebViewHandler>(handler);
		}

		public void SendRawMessage(string rawMessage)
		{
#pragma warning disable CA1416 // Validate platform compatibility
			PostWebMessage(new WebMessage(rawMessage), AndroidAppOriginUri);
#pragma warning restore CA1416 // Validate platform compatibility
		}
	}
}
