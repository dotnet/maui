using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

// By default, xUnit will run test collections (the tests in each class) in parallel
// with other test collections. Unfortunately, a _ton_ of the Controls legacy tests
// interact with properties on static classes (e.g., Application.Current), and if we 
// let them run in parallel they'll step on one another. So we tell xUnit to consider
// the whole assembly as a single collection for now, so all the tests run in sequence.
// (Hopefully in the future we can untangle some of the singletons and run these in parallel,
// because it'll be a lot faster.)
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]

namespace Microsoft.Maui.Controls.Core.UnitTests
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
			{
				return;
			}

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
