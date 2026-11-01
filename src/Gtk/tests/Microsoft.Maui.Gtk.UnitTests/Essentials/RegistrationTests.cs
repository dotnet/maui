using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Accessibility;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Essentials.Gtk;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests.Essentials;

/// <summary>
/// The registrations made by <c>UseMauiGtkEssentials</c> and the static facades they are bridged to.
/// Bridging mutates process-wide statics, so these tests share a collection and run serially.
/// </summary>
[Collection("EssentialsStatics")]
public class RegistrationTests
{
	static MauiApp Build(Action<GtkEssentialsOptions>? configure = null)
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiGtkEssentials(o =>
		{
			o.ApplicationId = "org.maui.gtk.tests";
			o.ApplicationName = "Gtk Tests";
			configure?.Invoke(o);
		});
		var app = builder.Build();
		GtkHost.Instance?.Invoke(() => { }); // the GTK thread becomes the main thread again
		return app;
	}

	[Fact]
	public void RegistersEverythingEssentialsCanUse()
	{
		var sp = Build().Services;

		Assert.IsType<GtkAppInfo>(sp.GetRequiredService<IAppInfo>());
		Assert.IsType<GtkDeviceInfo>(sp.GetRequiredService<IDeviceInfo>());
		Assert.IsType<GtkDeviceDisplay>(sp.GetRequiredService<IDeviceDisplay>());
		Assert.IsType<GtkFileSystem>(sp.GetRequiredService<IFileSystem>());
		Assert.IsType<GtkPreferences>(sp.GetRequiredService<IPreferences>());
		Assert.IsType<GtkSecureStorage>(sp.GetRequiredService<ISecureStorage>());
		Assert.IsType<GtkConnectivity>(sp.GetRequiredService<IConnectivity>());
		Assert.IsType<GtkBattery>(sp.GetRequiredService<IBattery>());
		Assert.IsType<GtkClipboard>(sp.GetRequiredService<IClipboard>());
		Assert.IsType<GtkLauncher>(sp.GetRequiredService<ILauncher>());
		Assert.IsType<GtkBrowser>(sp.GetRequiredService<IBrowser>());
		Assert.IsType<GtkEmail>(sp.GetRequiredService<IEmail>());
		Assert.IsType<GtkMap>(sp.GetRequiredService<IMap>());
		Assert.IsType<GtkFilePicker>(sp.GetRequiredService<IFilePicker>());
		Assert.IsType<GtkMediaPicker>(sp.GetRequiredService<IMediaPicker>());
		Assert.IsType<GtkScreenshot>(sp.GetRequiredService<IScreenshot>());
		Assert.IsType<GtkSemanticScreenReader>(sp.GetRequiredService<ISemanticScreenReader>());
		Assert.IsType<GtkShare>(sp.GetRequiredService<IShare>());
		Assert.IsType<UnsupportedGeolocation>(sp.GetRequiredService<IGeolocation>());
	}

	[Fact]
	public void StaticFacadesPointAtTheGtkImplementations()
	{
		Build();

		Assert.Equal("org.maui.gtk.tests", AppInfo.PackageName);
		Assert.Equal("Gtk Tests", AppInfo.Name);
		Assert.Equal("Linux", DeviceInfo.Platform.ToString());
		Assert.Equal(DeviceIdiom.Desktop, DeviceInfo.Idiom);
		Assert.IsType<GtkPreferences>(Preferences.Default);
		Assert.IsType<GtkFileSystem>(FileSystem.Current);
		Assert.False(Vibration.Default.IsSupported);
		Assert.False(Accelerometer.Default.IsSupported);
		Assert.False(PhoneDialer.Default.IsSupported);
		Assert.False(MediaPicker.Default.IsCaptureSupported);
	}

	[Fact]
	public void BuiltInFilePickerFileTypesWorkOnTheGtkPlatform()
	{
		Build();

		Assert.Contains("image/png", FilePickerFileType.Images.Value);
		Assert.Contains("application/pdf", FilePickerFileType.Pdf.Value);
		Assert.Equal(new[] { "image/jpeg" }, FilePickerFileType.Jpeg.Value);
	}

	[Fact]
	public async Task UnsupportedFeaturesFailWithFeatureNotSupported()
	{
		Build();

		Assert.Throws<FeatureNotSupportedException>(() => Vibration.Default.Vibrate());
		Assert.Throws<FeatureNotSupportedException>(() => Accelerometer.Default.Start(SensorSpeed.Default));
		Assert.Throws<FeatureNotSupportedException>(() => PhoneDialer.Default.Open("123"));
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => Sms.Default.ComposeAsync(new SmsMessage("a", "b")));
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => TextToSpeech.Default.SpeakAsync("x"));
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => Geolocation.Default.GetLastKnownLocationAsync());
		Assert.False(await Flashlight.Default.IsSupportedAsync());
	}

	[Fact]
	public void ScreenshotHooksAreRegisteredUnderTheDispatchKeys()
	{
		var sp = (IKeyedServiceProvider)Build().Services;

		Assert.NotNull(sp.GetKeyedService<Func<object, Task<IScreenshotResult?>>>(ScreenshotDispatch.ViewCaptureKey));
		Assert.NotNull(sp.GetKeyedService<Func<object, Task<IScreenshotResult?>>>(ScreenshotDispatch.WindowCaptureKey));
	}

	[Fact]
	public void ExistingRegistrationsWin()
	{
		var builder = MauiApp.CreateBuilder();
		var custom = new GtkPreferences(System.IO.Path.GetTempPath());
		builder.Services.AddSingleton<IPreferences>(custom);
		builder.UseMauiGtkEssentials();

		Assert.Same(custom, builder.Build().Services.GetRequiredService<IPreferences>());
	}

	[Fact]
	public async Task EmailRejectsWhatMailtoCannotCarry()
	{
		var email = new GtkEmail();

		Assert.True(email.IsComposeSupported);
		await Assert.ThrowsAsync<FeatureNotSupportedException>(() => email.ComposeAsync(new EmailMessage { BodyFormat = EmailBodyFormat.Html }));
	}
}
