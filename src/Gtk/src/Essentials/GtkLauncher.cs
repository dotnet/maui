using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="ILauncher"/> over <c>Gtk.UriLauncher</c> / <c>Gtk.FileLauncher</c> (GTK 4.10+, the floor is 4.14). Both go through the
/// OpenURI/OpenFile desktop portal when available, so they also work inside Flatpak/Snap sandboxes.
/// </summary>
/// <remarks>Must be called once GTK is initialized; work is marshalled to the GTK main thread.</remarks>
sealed class GtkLauncher : ILauncher
{
	public Task<bool> CanOpenAsync(Uri uri)
	{
		if (uri is null)
			throw new ArgumentNullException(nameof(uri));

		return GtkDispatch.InvokeAsync(() =>
		{
			try
			{
				return new global::Gtk.UriLauncher(uri.AbsoluteUri).CanLaunch(GtkEssentialsContext.GetParentWindow()!);
			}
			catch (Exception)
			{
				return uri.Scheme is "http" or "https" or "file" or "mailto";
			}
		});
	}

	public Task<bool> OpenAsync(Uri uri)
	{
		if (uri is null)
			throw new ArgumentNullException(nameof(uri));

		return LaunchUriAsync(uri.AbsoluteUri);
	}

	public Task<bool> TryOpenAsync(Uri uri) =>
		TryAsync(() => OpenAsync(uri));

	public Task<bool> OpenAsync(OpenFileRequest request)
	{
		if (request?.File is null)
			throw new ArgumentNullException(nameof(request));

		return LaunchFileAsync(request.File.FullPath);
	}

	internal static Task<bool> LaunchFileAsync(string path) =>
		GtkAsync.Begin<global::Gtk.FileLauncher, bool>(
			callback =>
			{
				var launcher = new global::Gtk.FileLauncher(GLib.FileFactory.NewForPath(path));
				launcher.Launch(GtkEssentialsContext.GetParentWindow()!, null!, callback);
				return launcher;
			},
			(launcher, result) => launcher.LaunchFinish(result));

	internal static Task<bool> OpenContainingFolderAsync(string path) =>
		GtkAsync.Begin<global::Gtk.FileLauncher, bool>(
			callback =>
			{
				var launcher = new global::Gtk.FileLauncher(GLib.FileFactory.NewForPath(path));
				launcher.OpenContainingFolder(GtkEssentialsContext.GetParentWindow()!, null!, callback);
				return launcher;
			},
			(launcher, result) => launcher.OpenContainingFolderFinish(result));

	internal static Task<bool> LaunchUriAsync(string uri) =>
		GtkAsync.Begin<global::Gtk.UriLauncher, bool>(
			callback =>
			{
				var launcher = new global::Gtk.UriLauncher(uri);
				launcher.Launch(GtkEssentialsContext.GetParentWindow()!, null!, callback);
				return launcher;
			},
			(launcher, result) => launcher.LaunchFinish(result));

	internal static async Task<bool> TryAsync(Func<Task<bool>> open)
	{
		try
		{
			return await open().ConfigureAwait(false);
		}
		catch (Exception)
		{
			return false;
		}
	}
}

/// <summary><see cref="IBrowser"/> that opens the URI in the user's default browser (launch options other than the URI are ignored).</summary>
sealed class GtkBrowser : IBrowser
{
	public Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options)
	{
		if (uri is null)
			throw new ArgumentNullException(nameof(uri));

		return GtkLauncher.LaunchUriAsync(uri.AbsoluteUri);
	}
}

/// <summary><see cref="IEmail"/> that opens the default mail client with a <c>mailto:</c> URI. Attachments are not supported.</summary>
sealed class GtkEmail : IEmail
{
	public bool IsComposeSupported => true;

	public async Task ComposeAsync(EmailMessage? message)
	{
		message ??= new EmailMessage();

		if (message.Attachments is { Count: > 0 })
			throw new FeatureNotSupportedException("Email attachments are not supported on Linux: a mailto: URI cannot carry files.");

		if (message.BodyFormat == EmailBodyFormat.Html)
			throw new FeatureNotSupportedException("HTML email bodies are not supported on Linux: a mailto: URI carries plain text only.");

		await GtkLauncher.LaunchUriAsync(LaunchUris.Mailto(message)).ConfigureAwait(false);
	}
}

/// <summary><see cref="IMap"/> that opens the default maps application (GNOME Maps, Marble...) with a <c>geo:</c> URI.</summary>
sealed class GtkMap : IMap
{
	public async Task OpenAsync(double latitude, double longitude, MapLaunchOptions options)
	{
		if (options is null)
			throw new ArgumentNullException(nameof(options));

		await GtkLauncher.LaunchUriAsync(LaunchUris.Geo(latitude, longitude, options.Name)).ConfigureAwait(false);
	}

	public async Task OpenAsync(Placemark placemark, MapLaunchOptions options)
	{
		if (placemark is null)
			throw new ArgumentNullException(nameof(placemark));
		if (options is null)
			throw new ArgumentNullException(nameof(options));

		var uri = placemark.Location is { } location && (location.Latitude != 0 || location.Longitude != 0)
			? LaunchUris.Geo(location.Latitude, location.Longitude, string.IsNullOrEmpty(options.Name) ? placemark.FeatureName : options.Name)
			: LaunchUris.GeoQuery(placemark);

		if (uri is null)
			throw new ArgumentException("The placemark has neither a location nor address details.", nameof(placemark));

		await GtkLauncher.LaunchUriAsync(uri).ConfigureAwait(false);
	}

	public Task<bool> TryOpenAsync(double latitude, double longitude, MapLaunchOptions options) =>
		GtkLauncher.TryAsync(async () =>
		{
			await OpenAsync(latitude, longitude, options).ConfigureAwait(false);
			return true;
		});

	public Task<bool> TryOpenAsync(Placemark placemark, MapLaunchOptions options) =>
		GtkLauncher.TryAsync(async () =>
		{
			await OpenAsync(placemark, options).ConfigureAwait(false);
			return true;
		});
}
