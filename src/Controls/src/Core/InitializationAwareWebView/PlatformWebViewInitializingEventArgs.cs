#if IOS || MACCATALYST
using WebKit;
#elif ANDROID
using Android.Webkit;
#elif WINDOWS
using Microsoft.Web.WebView2.Core;
#endif

namespace Microsoft.Maui.Controls;

/// <summary>
/// Provides platform-specific information about the <see cref="WebViewInitializingEventArgs"/> event.
/// </summary>
public class PlatformWebViewInitializingEventArgs
{
	readonly WebViewInitializationStartedEventArgs _coreArgs;

	internal PlatformWebViewInitializingEventArgs(WebViewInitializationStartedEventArgs args)
	{
		_coreArgs = args;
	}

#if WINDOWS

	/// <summary>
	/// Gets or sets the relative path to the folder that contains a custom
	/// version of WebView2 Runtime.
	/// </summary>
	/// <remarks>
	/// To use a fixed version of the WebView2 Runtime, set this property to 
	/// the folder path that contains the fixed version of the WebView2 Runtime. 
	/// </remarks>
	public string? BrowserExecutableFolder
	{
		get => _coreArgs.BrowserExecutableFolder;
		set => _coreArgs.BrowserExecutableFolder = value;
	}

	/// <summary>
	/// Gets or sets the user data folder location for WebView2.
	/// </summary>
	/// <remarks>
	/// The default user data folder {Executable File Name}.WebView2 is created
	/// in the same directory next to the compiled code for the app.
	/// WebView2 creation fails if the compiled code is running in a directory
	/// in which the process does not have permission to create a new directory.
	/// The app is responsible to clean up the associated user data folder
	/// when it is done.
	/// </remarks>
	public string? UserDataFolder
	{
		get => _coreArgs.UserDataFolder;
		set => _coreArgs.UserDataFolder = value;
	}

	/// <summary>
	/// Gets or sets the options used to create WebView2 Environment.
	/// </summary>
	/// <remarks>
	/// As a browser process may be shared among WebViews, WebView creation fails
	/// if the specified options does not match the options of the WebViews
	/// that are currently running in the shared browser process.
	/// </remarks>
	public CoreWebView2EnvironmentOptions? EnvironmentOptions
	{
		get => _coreArgs.EnvironmentOptions;
		set => _coreArgs.EnvironmentOptions = value;
	}

	/// <summary>
	/// Gets or sets whether the WebView2 controller is in private mode.
	/// </summary>
	public bool IsInPrivateModeEnabled
	{
		get => _coreArgs.IsInPrivateModeEnabled;
		set => _coreArgs.IsInPrivateModeEnabled = value;
	}

	/// <summary>
	/// Gets or sets the name of the controller profile.
	/// </summary>
	/// <remarks>
	/// Profile names are only allowed to contain the following ASCII characters:
	/// * alphabet characters: a-z and A-Z
	/// * digit characters: 0-9
	/// * and '#', '@', '$', '(', ')', '+', '-', '_', '~', '.', ' ' (space).
	/// It has a maximum length of 64 characters excluding the null-terminator.
	/// It is ASCII case insensitive.
	/// </remarks>
	public string? ProfileName
	{
		get => _coreArgs.ProfileName;
		set => _coreArgs.ProfileName = value;
	}

	/// <summary>
	/// Gets or sets the controller's default script locale.
	/// </summary>
	/// <remarks>
	/// This property sets the default locale for all Intl JavaScript APIs and other JavaScript APIs that
	/// depend on it, namely Intl.DateTimeFormat() which affects string formatting like in the time/date
	/// formats. The intended locale value is in the format of BCP 47 Language Tags. 
	/// More information can be found from https://www.ietf.org/rfc/bcp/bcp47.html. 
	/// The default value for ScriptLocale will be depend on the WebView2 language and OS region.
	/// If the language portions of the WebView2 language and OS region match, then it will use the OS region.
	/// Otherwise, it will use the WebView2 language.
	/// </remarks>
	public string? ScriptLocale
	{
		get => _coreArgs.ScriptLocale;
		set => _coreArgs.ScriptLocale = value;
	}

#elif IOS || MACCATALYST

	/// <summary>
	/// Gets or sets the configuration to be used in the construction of the WKWebView instance.
	/// </summary>
	public WKWebViewConfiguration Configuration => _coreArgs.Configuration;

#elif ANDROID

	/// <summary>
	/// Gets the platform-specific settings for the WebView.
	/// </summary>
	public WebSettings Settings => _coreArgs.Settings;

#else

#endif
}
