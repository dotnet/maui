using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BaseTestFixture
	{
		CultureInfo _defaultCulture;
		CultureInfo _defaultUICulture;

		[SetUp]
		public virtual void Setup()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			MockPlatformSizeService.Current?.Reset();
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		[TearDown]
		public virtual void TearDown()
		{
			MockPlatformSizeService.Current?.Reset();
			AppInfo.SetCurrent(null);
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
