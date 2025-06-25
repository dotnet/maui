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
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class BaseTestFixture : IDisposable
	{
		CultureInfo _defaultCulture;
		CultureInfo _defaultUICulture;

		public BaseTestFixture()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceDisplay.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			AppInfo.SetCurrent(null);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool _disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				AppInfo.SetCurrent(null);
				DeviceDisplay.SetCurrent(null);
				DeviceInfo.SetCurrent(null);
				System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
				System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
				DispatcherProvider.SetCurrent(null);
			}

			_disposed = true;
		}
	}
}
