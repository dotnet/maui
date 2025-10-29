using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class BaseTestFixture
	{
		CultureInfo _defaultCulture;
		CultureInfo _defaultUICulture;

		protected virtual void Setup()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		protected virtual void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
