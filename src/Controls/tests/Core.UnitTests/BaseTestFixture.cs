using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Essentials;
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
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			Device.PlatformServices = new MockPlatformServices();
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[TearDown]
		public virtual void TearDown()
		{
			Device.PlatformServices = null;
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
