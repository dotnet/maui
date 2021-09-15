using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Essentials;
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
			DeviceInfo.SetCurrent(new MockDeviceInfo());
		}

		[TearDown]
		public virtual void TearDown()
		{
			DeviceInfo.SetCurrent(null);
			Device.PlatformServices = null;
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
		}
	}
}
