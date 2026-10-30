using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Hosting;

/// <summary>
/// Registers the Linux/GTK 4 implementations of the .NET MAUI Essentials APIs.
/// </summary>
public static class GtkEssentialsExtensions
{
	/// <summary>
	/// Registers the GTK implementations of the Essentials services in dependency injection. MAUI's Essentials initializer
	/// then bridges every registered service to its static facade (<c>Preferences.Default</c>, <c>DeviceInfo.Current</c>...).
	/// </summary>
	/// <param name="builder">The app builder.</param>
	/// <param name="configure">Optional callback to set <see cref="GtkEssentialsOptions"/>.</param>
	/// <returns>The same <paramref name="builder"/>.</returns>
	/// <remarks>
	/// <para>Call it on the GTK main thread (normally from <c>CreateMauiApp</c>, which runs in <c>Main</c>): that thread is
	/// remembered as the target of every API that needs the GTK main loop (clipboard, launchers, dialogs, screenshots).
	/// Services that already have a registration are left alone, so an app can replace any of them.</para>
	/// <para>Registered: AppInfo, DeviceInfo, DeviceDisplay, FileSystem, Preferences, SecureStorage (libsecret), Connectivity,
	/// Battery, Clipboard, Launcher, Browser, Email, Map, FilePicker, MediaPicker, Screenshot (also the keyed view/window
	/// capture hooks used by <c>CaptureAsync</c>), SemanticScreenReader, and unsupported-feature stubs for sensors, Contacts,
	/// Flashlight, Vibration, HapticFeedback, Sms, PhoneDialer, Share, TextToSpeech and Geolocation.</para>
	/// </remarks>
	public static MauiAppBuilder UseMauiGtkEssentials(this MauiAppBuilder builder, Action<GtkEssentialsOptions>? configure = null)
	{
		if (builder is null)
			throw new ArgumentNullException(nameof(builder));

		var options = new GtkEssentialsOptions();
		configure?.Invoke(options);

		GtkEssentialsContext.Options = options;
		GtkDispatch.MarkCurrentThreadAsMain();

		var identity = AppIdentity.Resolve(GtkEssentialsContext.EntryAssembly, options.ApplicationId, options.ApplicationName);
		var services = builder.Services;

		services.TryAddSingleton(options);
		services.TryAddSingleton<IAppInfo>(new GtkAppInfo(identity));
		services.TryAddSingleton<IDeviceInfo>(_ => new GtkDeviceInfo());
		services.TryAddSingleton<IDeviceDisplay>(_ => new GtkDeviceDisplay());
		services.TryAddSingleton<IFileSystem>(new GtkFileSystem(identity.ApplicationId));
		services.TryAddSingleton<IPreferences>(new GtkPreferences(identity.ApplicationId));
		services.TryAddSingleton<ISecureStorage>(new GtkSecureStorage(identity.ApplicationId));
		services.TryAddSingleton<IConnectivity>(_ => new GtkConnectivity());
		services.TryAddSingleton<IBattery>(_ => new GtkBattery());
		services.TryAddSingleton<IClipboard>(_ => new GtkClipboard());
		services.TryAddSingleton<ILauncher>(_ => new GtkLauncher());
		services.TryAddSingleton<IBrowser>(_ => new GtkBrowser());
		services.TryAddSingleton<IEmail>(_ => new GtkEmail());
		services.TryAddSingleton<IMap>(_ => new GtkMap());
		services.TryAddSingleton<IFilePicker>(_ => new GtkFilePicker());
		services.TryAddSingleton<IMediaPicker>(_ => new GtkMediaPicker());
		services.TryAddSingleton<IScreenshot>(_ => new GtkScreenshot());
		services.TryAddSingleton<ISemanticScreenReader>(_ => new GtkSemanticScreenReader());

		Func<object, Task<IScreenshotResult?>> capture = GtkScreenshot.CaptureAsync;
		services.TryAddKeyedSingleton(ScreenshotDispatch.ViewCaptureKey, capture);
		services.TryAddKeyedSingleton(ScreenshotDispatch.WindowCaptureKey, capture);

		services.TryAddSingleton<IAccelerometer>(_ => new UnsupportedAccelerometer());
		services.TryAddSingleton<IBarometer>(_ => new UnsupportedBarometer());
		services.TryAddSingleton<ICompass>(_ => new UnsupportedCompass());
		services.TryAddSingleton<IGyroscope>(_ => new UnsupportedGyroscope());
		services.TryAddSingleton<IMagnetometer>(_ => new UnsupportedMagnetometer());
		services.TryAddSingleton<IOrientationSensor>(_ => new UnsupportedOrientationSensor());
		services.TryAddSingleton<IContacts>(_ => new UnsupportedContacts());
		services.TryAddSingleton<IFlashlight>(_ => new UnsupportedFlashlight());
		services.TryAddSingleton<IVibration>(_ => new UnsupportedVibration());
		services.TryAddSingleton<IHapticFeedback>(_ => new UnsupportedHapticFeedback());
		services.TryAddSingleton<ISms>(_ => new UnsupportedSms());
		services.TryAddSingleton<IPhoneDialer>(_ => new UnsupportedPhoneDialer());
		services.TryAddSingleton<IShare>(_ => new UnsupportedShare());
		services.TryAddSingleton<ITextToSpeech>(_ => new UnsupportedTextToSpeech());
		services.TryAddSingleton<IGeolocation>(_ => new UnsupportedGeolocation());

		return builder;
	}
}
