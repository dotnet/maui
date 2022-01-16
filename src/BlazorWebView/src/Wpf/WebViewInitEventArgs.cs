// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
	/// <summary>
	/// Event arguments for the InitializingWebView event.
	/// </summary>
	public sealed class WebViewInitEventArgs : RoutedEventArgs
	{
		/// <summary>
		/// Creates the event args.
		/// </summary>
		/// <param name="coreWebView2EnvironmentOptions"></param>
		internal WebViewInitEventArgs(CoreWebView2EnvironmentOptions coreWebView2EnvironmentOptions)
		{
			CoreWebView2EnvironmentOptions = coreWebView2EnvironmentOptions;
		}

		/// <summary>
		/// Options used to create WebView2 Environment.
		/// As a browser process may be shared among WebViews, WebView creation fails if
		/// the specified options does not match the options of the WebViews that are currently
		/// running in the shared browser process.
		/// </summary>
		public CoreWebView2EnvironmentOptions CoreWebView2EnvironmentOptions { get; }

		/// <summary>
		/// The relative path to the folder that contains a custom version of WebView2 Runtime.
		/// To use a fixed version of the WebView2 Runtime, pass the folder path that contains
		/// the fixed version of the WebView2 Runtime to browserExecutableFolder. BrowserExecutableFolder
		/// supports both relative (to the application's executable) and absolute file paths.
		/// To create WebView2 controls that use the installed version of the WebView2 Runtime
		/// that exists on user machines, pass a null or empty string to browserExecutableFolder.
		/// In this scenario, the API tries to find a compatible version of the WebView2
		/// Runtime that is installed on the user machine (first at the machine level, and
		/// then per user) using the selected channel preference. The path of fixed version
		/// of the WebView2 Runtime should not contain \Edge\Application\. When such a path
		/// is used, the API fails with ERROR_NOT_SUPPORTED.
		/// </summary>
		public string CoreWebView2BrowserExecutableFolder { get; set; } = null;

		/// <summary>
		/// The user data folder location for WebView2.
		/// The path is either an absolute file path or a relative file path that is interpreted
		/// as relative to the compiled code for the current process. The default user data
		/// folder {Executable File Name}.WebView2 is created in the same directory next
		/// to the compiled code for the app. WebView2 creation fails if the compiled code
		/// is running in a directory in which the process does not have permission to create
		/// a new directory. The app is responsible to clean up the associated user data
		/// folder when it is done.
		/// </summary>
		public string CoreWebView2UserDataFolder { get; set; } = null;

		protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
		{
			var handler = (InitializingWebViewEventHandler)genericHandler;
			handler(genericTarget, this);
		}
	}
}
