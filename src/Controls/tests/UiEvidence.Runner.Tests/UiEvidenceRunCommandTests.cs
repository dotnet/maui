using System.Diagnostics;
using OpenQA.Selenium.Appium;
using UITest.Appium;
using UITest.Core;
using Xunit;

namespace Microsoft.Maui.UiEvidence;

public class UiEvidenceRunCommandTests
{
	[Fact]
	public void AndroidTarget_IsRequiredAndCannotBeAnImplicitFlag()
	{
		foreach (var arguments in new[]
		{
			Array.Empty<string>(),
			new[] { "--device-id" },
			new[] { "--device-id", " " },
			new[] { "--device-id", "-d" }
		})
			Assert.Throws<ArgumentException>(() => UiEvidenceRunCommand.RequiredDeviceId(CommandLineOptions.Parse(arguments)));
	}

	[Fact]
	public void AndroidTarget_BindsTheAppiumUdidToTheAdbSerial()
	{
		var options = CommandLineOptions.Parse(["--variant", "base", "--device-id", "emulator-5588"]);
		var config = UiEvidenceRunCommand.CreateAppConfig("android", "app.apk", "output", options);
		config.SetProperty("PlatformName", "Android");
		var capabilities = OptionsProbe.Build(config).ToCapabilities();

		Assert.Equal(UiEvidenceRunCommand.RequiredDeviceId(options), capabilities.GetCapability("appium:udid"));
	}

	[Fact]
	public void GeneralAppiumOptions_PreserveIosUdidAndOmitUnspecifiedUdid()
	{
		var config = new Config();
		config.SetProperty("PlatformName", "iOS");
		Assert.Null(OptionsProbe.Build(config).ToCapabilities().GetCapability("appium:udid"));
		config.SetProperty("Udid", "existing-ios-device");
		Assert.Equal("existing-ios-device", OptionsProbe.Build(config).ToCapabilities().GetCapability("appium:udid"));
	}

	[Theory]
	[InlineData("Android", true)]
	[InlineData("android", true)]
	[InlineData("iOS", true)]
	[InlineData("IOS", true)]
	[InlineData("Windows", false)]
	[InlineData("mac", false)]
	[InlineData("MacCatalyst", false)]
	[InlineData("", false)]
	public void GeneralAppiumOptions_OnlyMobilePlatformsReceiveUdid(string platform, bool expected)
	{
		var config = new Config();
		config.SetProperty("PlatformName", platform);
		config.SetProperty("Udid", "configured-device");
		var capabilities = OptionsProbe.Build(config).ToCapabilities();

		Assert.Equal(expected ? "configured-device" : null, capabilities.GetCapability("appium:udid"));
	}

	[Fact]
	public void WindowsOwnershipGuard_DoesNotTerminateAnExistingProcess()
	{
		using var current = Process.GetCurrentProcess();
		var error = Assert.Throws<InvalidOperationException>(() =>
			UiEvidenceRunCommand.EnsureWindowsAppNotRunning(Environment.ProcessPath!));
		Assert.Contains("unowned process", error.Message, StringComparison.Ordinal);
		Assert.False(current.HasExited);
	}

	abstract class OptionsProbe : AppiumApp
	{
		protected OptionsProbe(AppiumDriver driver, IConfig config) : base(driver, config) { }

		public static AppiumOptions Build(IConfig config)
		{
			var options = new AppiumOptions();
			SetGeneralAppiumOptions(config, options);
			return options;
		}
	}
}
