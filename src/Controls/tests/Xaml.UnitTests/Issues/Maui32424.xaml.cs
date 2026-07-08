using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui32424 : Shell
{
	public Maui32424() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		readonly MockAppInfo _appInfo;
		readonly MockApplication _app;
		readonly MockDeviceInfo _deviceInfo;

		public Tests()
		{
			AppInfo.SetCurrent(_appInfo = new MockAppInfo());
			Application.Current = _app = new MockApplication();
			DeviceInfo.SetCurrent(_deviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			DeviceInfo.SetCurrent(null);
			Application.Current = null;
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformDefaultCanBeNestedAppThemeBinding(XamlInflator inflator)
		{
			_deviceInfo.Platform = DevicePlatform.iOS;

			_appInfo.RequestedTheme = AppTheme.Light;
			_app.NotifyThemeChanged();
			var shell = new Maui32424(inflator);
			Assert.Equal(Colors.White, shell.FlyoutBackgroundColor);

			_appInfo.RequestedTheme = AppTheme.Dark;
			_app.NotifyThemeChanged();
			shell = new Maui32424(inflator);
			Assert.Equal(Colors.Black, shell.FlyoutBackgroundColor);
		}

		[Fact]
		internal void SourceGenForNetStandardDoesNotReferenceInternalAppThemeBinding()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
					"""
					namespace Microsoft.Maui.Controls.Xaml.UnitTests;

					public partial class Maui32424 : Microsoft.Maui.Controls.Shell
					{
						public Maui32424() => InitializeComponent();
					}
					""")
				.RunMauiSourceGenerator(typeof(Maui32424), targetFramework: "netstandard2.0");

			Assert.Empty(result.Diagnostics);

			var generated = result.GeneratedInitializeComponent();
			Assert.DoesNotContain("new global::Microsoft.Maui.Controls.AppThemeBinding", generated, StringComparison.Ordinal);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformMatchingPlatformStillUsesPlatformValue(XamlInflator inflator)
		{
			_deviceInfo.Platform = DevicePlatform.WinUI;
			_appInfo.RequestedTheme = AppTheme.Dark;

			var shell = new Maui32424(inflator);

			Assert.Equal(Colors.Transparent, shell.FlyoutBackgroundColor);
		}
	}
}
