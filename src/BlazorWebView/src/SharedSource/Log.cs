using System;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Components.WebView;

internal static partial class Log
{
	[LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Navigating to {uri}.")]
	public static partial void NavigatingToUri(this ILogger logger, Uri uri);

	[LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "Failed to create WebView2 environment. This could mean that WebView2 is not installed.")]
	public static partial void FailedToCreateWebView2Environment(this ILogger logger);

	[LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Starting WebView2...")]
	public static partial void StartingWebView2(this ILogger logger);

	[LoggerMessage(EventId = 3, Level = LogLevel.Debug, Message = "WebView2 is started.")]
	public static partial void StartedWebView2(this ILogger logger);

	[LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Handling web request to URI '{requestUri}'.")]
	public static partial void HandlingWebRequest(this ILogger logger, string requestUri);

	[LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Response content being sent for web request to URI '{requestUri}' with HTTP status code {statusCode}.")]
	public static partial void ResponseContentBeingSent(this ILogger logger, string requestUri, int statusCode);

	[LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Response content was not found for web request to URI '{requestUri}'.")]
	public static partial void ResponseContentNotFound(this ILogger logger, string requestUri);

	[LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Navigation event for URI '{uri}' with URL loading strategy '{urlLoadingStrategy}'.")]
	public static partial void NavigationEvent(this ILogger logger, Uri uri, UrlLoadingStrategy urlLoadingStrategy);

	[LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Launching external browser for URI '{uri}'.")]
	public static partial void LaunchExternalBrowser(this ILogger logger, Uri uri);

	[LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "Calling Blazor.start() in the WebView2.")]
	public static partial void CallingBlazorStart(this ILogger logger);

	[LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Creating file provider at content root '{contentRootDir}', using host page relative path '{hostPageRelativePath}'.")]
	public static partial void CreatingFileProvider(this ILogger logger, string contentRootDir, string hostPageRelativePath);

	[LoggerMessage(EventId = 11, Level = LogLevel.Debug, Message = "Adding root component '{componentTypeName}' with selector '{componentSelector}'. Number of parameters: {parameterCount}")]
	public static partial void AddingRootComponent(this ILogger logger, string componentTypeName, string componentSelector, int parameterCount);

	[LoggerMessage(EventId = 12, Level = LogLevel.Debug, Message = "Starting initial navigation to '{startPath}'.")]
	public static partial void StartingInitialNavigation(this ILogger logger, string startPath);

	[LoggerMessage(EventId = 13, Level = LogLevel.Debug, Message = "Creating Android.Webkit.WebView...")]
	public static partial void CreatingAndroidWebkitWebView(this ILogger logger);

	[LoggerMessage(EventId = 14, Level = LogLevel.Debug, Message = "Created Android.Webkit.WebView.")]
	public static partial void CreatedAndroidWebkitWebView(this ILogger logger);

	[LoggerMessage(EventId = 15, Level = LogLevel.Debug, Message = "Running Blazor startup scripts.")]
	public static partial void RunningBlazorStartupScripts(this ILogger logger);

	[LoggerMessage(EventId = 16, Level = LogLevel.Debug, Message = "Blazor startup scripts finished.")]
	public static partial void BlazorStartupScriptsFinished(this ILogger logger);

	[LoggerMessage(EventId = 17, Level = LogLevel.Debug, Message = "Creating WebKit WKWebView...")]
	public static partial void CreatingWebKitWKWebView(this ILogger logger);

	[LoggerMessage(EventId = 18, Level = LogLevel.Debug, Message = "Created WebKit WKWebView.")]
	public static partial void CreatedWebKitWKWebView(this ILogger logger);
}
