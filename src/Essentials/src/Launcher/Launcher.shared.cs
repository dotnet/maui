#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// The Launcher API enables an application to open a URI by the system.
	/// This is often used when deep linking into another application's custom URI schemes.
	/// </summary>
	/// <remarks>
	/// <para>If you are looking to open the browser to a website then you should refer to the <see cref="IBrowser"/> API.</para>
	/// <para>On iOS 9+, you will have to specify the <c>LSApplicationQueriesSchemes</c> key in the <c>info.plist</c> file with URI schemes you want to query from your app.</para>
	/// </remarks>
	public interface ILauncher
	{
		/// <summary>
		/// Queries if the device supports opening the given URI scheme.
		/// </summary>
		/// <param name="uri">URI scheme to query.</param>
		/// <returns><see langword="true"/> if opening is supported, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		Task<bool> CanOpenAsync(Uri uri);

		/// <summary>
		/// Opens the app specified by the URI scheme.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		Task<bool> OpenAsync(Uri uri);

		/// <summary>
		/// Requests to open a file in an application based on content type.
		/// </summary>
		/// <param name="request">Request that contains information on the file to open.</param>
		/// <returns><see langword="true"/> if the file was opened, otherwise <see langword="false"/>.</returns>
		Task<bool> OpenAsync(OpenFileRequest request);

		/// <summary>
		/// First checks if the provided URI is supported, then opens the app specified by the URI.
		/// </summary>
		/// <param name="uri">URI to try and open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		Task<bool> TryOpenAsync(Uri uri);
	}

	/// <summary>
	/// The Launcher API enables an application to open a URI by the system.
	/// This is often used when deep linking into another application's custom URI schemes.
	/// </summary>
	/// <remarks>
	/// <para>If you are looking to open the browser to a website then you should refer to the <see cref="IBrowser"/> API.</para>
	/// <para>On iOS 9+, you will have to specify the <c>LSApplicationQueriesSchemes</c> key in the <c>info.plist</c> file with URI schemes you want to query from your app.</para>
	/// </remarks>
	public static partial class Launcher
	{
		/// <summary>
		/// Queries if the device supports opening the given URI scheme.
		/// </summary>
		/// <param name="uri">URI scheme to query.</param>
		/// <returns><see langword="true"/> if opening is supported, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> CanOpenAsync(string uri)
			=> Current.CanOpenAsync(uri);

		/// <summary>
		/// Queries if the device supports opening the given URI scheme.
		/// </summary>
		/// <param name="uri">URI scheme to query.</param>
		/// <returns><see langword="true"/> if opening is supported, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> CanOpenAsync(Uri uri)
			=> Current.CanOpenAsync(uri);

		/// <summary>
		/// Opens the app specified by the URI scheme.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> OpenAsync(string uri)
			=> Current.OpenAsync(uri);

		/// <summary>
		/// Opens the app specified by the URI scheme.
		/// </summary>
		/// <param name="uri">URI to open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> OpenAsync(Uri uri)
			=> Current.OpenAsync(uri);

		/// <summary>
		/// Requests to open a file in an application based on content type.
		/// </summary>
		/// <param name="request">Request that contains information on the file to open.</param>
		/// <returns><see langword="true"/> if the file was opened, otherwise <see langword="false"/>.</returns>
		public static Task<bool> OpenAsync(OpenFileRequest request)
			=> Current.OpenAsync(request);

		/// <summary>
		/// First checks if the provided URI is supported, then opens the app specified by the URI.
		/// </summary>
		/// <param name="uri">URI to try and open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> TryOpenAsync(string uri)
			=> Current.TryOpenAsync(uri);

		/// <summary>
		/// First checks if the provided URI is supported, then opens the app specified by the URI.
		/// </summary>
		/// <param name="uri">URI to try and open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> TryOpenAsync(Uri uri)
			=> Current.TryOpenAsync(uri);

		static ILauncher Current => ApplicationModel.Launcher.Default;

		static ILauncher? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ILauncher Default =>
			defaultImplementation ??= new LauncherImplementation();

		internal static void SetDefault(ILauncher? implementation) =>
			defaultImplementation = implementation;
	}

	partial class LauncherImplementation : ILauncher
	{
		public Task<bool> CanOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformCanOpenAsync(uri);
		}

		public Task<bool> OpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformOpenAsync(uri);
		}

		public Task<bool> OpenAsync(OpenFileRequest request)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));
			if (request.File == null)
				throw new ArgumentNullException(nameof(request.File));

			return PlatformOpenAsync(request);
		}

		public Task<bool> TryOpenAsync(Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			return PlatformTryOpenAsync(uri);
		}
	}

	/// <summary>
	/// Static class with extension methods for the <see cref="ILauncher"/> APIs.
	/// </summary>
	public static class LauncherExtensions
	{
		/// <summary>
		/// Queries if the device supports opening the given URI scheme.
		/// </summary>
		/// <param name="launcher">The object this method is invoked on.</param>
		/// <param name="uri">URI scheme to query.</param>
		/// <returns><see langword="true"/> if opening is supported, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> CanOpenAsync(this ILauncher launcher, string uri) =>
			launcher.CanOpenAsync(new Uri(uri));

		/// <summary>
		/// Opens the app specified by the URI scheme.
		/// </summary>
		/// <param name="launcher">The object this method is invoked on.</param>
		/// <param name="uri">URI to open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> OpenAsync(this ILauncher launcher, string uri) =>
			launcher.OpenAsync(new Uri(uri));

		/// <summary>
		/// First checks if the provided URI is supported, then opens the app specified by the URI.
		/// </summary>
		/// <param name="launcher">The object this method is invoked on.</param>
		/// <param name="uri">URI to try and open.</param>
		/// <returns><see langword="true"/> if the URI was opened, otherwise <see langword="false"/>.</returns>
		/// <exception cref="UriFormatException">Thrown when <paramref name="uri"/> is malformed.</exception>
		public static Task<bool> TryOpenAsync(this ILauncher launcher, string uri) =>
			launcher.TryOpenAsync(new Uri(uri));
	}

	/// <summary>
	/// Represents a request for opening a file in another application.
	/// </summary>
	public class OpenFileRequest
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="OpenFileRequest"/> class.
		/// </summary>
		public OpenFileRequest()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenFileRequest"/> class with the given title and existing file.
		/// </summary>
		/// <param name="title">Title to display on open dialog.</param>
		/// <param name="file">File to open.</param>
		/// <remarks>The title might not always be displayed on every platform.</remarks>
		public OpenFileRequest(string title, ReadOnlyFile file)
		{
			Title = title;
			File = file;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenFileRequest"/> class with the given title and existing file.
		/// </summary>
		/// <param name="title">Title to display on the open dialog.</param>
		/// <param name="file">File to open.</param>
		/// <remarks>The title might not always be displayed on every platform.</remarks>
		public OpenFileRequest(string title, FileBase file)
		{
			Title = title;
			File = new ReadOnlyFile(file);
		}

		/// <summary>
		/// Gets or sets the title to display on the open dialog.
		/// </summary>
		/// <remarks>The title might not always be displayed on every platform.</remarks>
		public string? Title { get; set; }

		/// <summary>
		/// Gets or sets the file to open through this request.
		/// </summary>
		public ReadOnlyFile? File { get; set; }

		/// <summary>
		/// Gets or sets the source rectangle to display the Share UI from.
		/// </summary>
		/// <remarks>This only has effect on iPadOS.</remarks>
		public Rect PresentationSourceBounds { get; set; } = Rect.Zero;
	}
}