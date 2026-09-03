#nullable enable
using System;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Represents information about the application.
	/// </summary>
	public interface IAppInfo
	{
		/// <summary>
		/// Gets the application package name or identifier.
		/// </summary>
		/// <remarks>On Android and iOS, this is the application package name. On Windows, this is the application GUID.</remarks>
		string PackageName { get; }

		/// <summary>
		/// Gets the application name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets the application version as a string representation.
		/// </summary>
		string VersionString { get; }

		/// <summary>
		/// Gets the application version as a <see cref="Version"/> object.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// Gets the application build number.
		/// </summary>
		string BuildString { get; }

		/// <summary>
		/// Open the settings menu or page for this application.
		/// </summary>
		void ShowSettingsUI();

		/// <summary>
		/// Gets the detected theme of the system or application.
		/// </summary>
		/// <remarks>For platforms or platform versions which do not support themes, <see cref="AppTheme.Unspecified"/> is returned.</remarks>
		AppTheme RequestedTheme { get; }

		/// <summary>
		/// Gets the packaging model of this application.
		/// </summary>
		/// <remarks>On other platforms than Windows, this will always return <see cref="AppPackagingModel.Packaged"/>.</remarks>
		AppPackagingModel PackagingModel { get; }

		/// <summary>
		/// Gets the requested layout direction of the system or application.
		/// </summary>
		LayoutDirection RequestedLayoutDirection { get; }
	}

	/// <summary>
	/// Represents information about the application.
	/// </summary>
	public static class AppInfo
	{
		/// <summary>
		/// Gets the application package name or identifier.
		/// </summary>
		/// <remarks>On Android and iOS, this is the application package name. On Windows, this is the application GUID.</remarks>
		public static string PackageName => Current.PackageName;

		/// <summary>
		/// Gets the application name.
		/// </summary>
		public static string Name => Current.Name;

		/// <summary>
		/// Gets the application version as a string representation.
		/// </summary>
		public static string VersionString => Current.VersionString;

		/// <summary>
		/// Gets the application version as a <see cref="Version"/> object.
		/// </summary>
		public static Version Version => Current.Version;

		/// <summary>
		/// Gets the application build number.
		/// </summary>
		public static string BuildString => Current.BuildString;

		/// <summary>
		/// Open the settings menu or page for this application.
		/// </summary>
		public static void ShowSettingsUI() => Current.ShowSettingsUI();

		/// <summary>
		/// Gets the detected theme of the system or application.
		/// </summary>
		/// <remarks>For platforms or platform versions which do not support themes, <see cref="AppTheme.Unspecified"/> is returned.</remarks>
		public static AppTheme RequestedTheme => Current.RequestedTheme;

		/// <summary>
		/// Gets the packaging model of this application.
		/// </summary>
		/// <remarks>On other platforms than Windows, this will always return <see cref="AppPackagingModel.Packaged"/>.</remarks>
		public static AppPackagingModel PackagingModel => Current.PackagingModel;

		/// <summary>
		/// Gets the requested layout direction of the system or application.
		/// </summary>
		public static LayoutDirection RequestedLayoutDirection => Current.RequestedLayoutDirection;

		static IAppInfo? currentImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IAppInfo Current =>
			currentImplementation ??= new AppInfoImplementation();

		internal static void SetCurrent(IAppInfo? implementation) =>
			currentImplementation = implementation;
	}

	/// <summary>
	/// Describes packaging options for a Windows app.
	/// </summary>
	public enum AppPackagingModel
	{
		/// <summary>The app is packaged and can be distributed through an MSIX or the store.</summary>
		Packaged,

		/// <summary>The app is unpackaged and can be distributed as a collection of executable files.</summary>
		Unpackaged,
	}
}
