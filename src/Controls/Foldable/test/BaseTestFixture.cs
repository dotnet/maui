using System;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Microsoft.Maui.Controls.Foldable.UnitTests
{
	public class BaseTestFixture : IDisposable
	{
		CultureInfo _defaultCulture;
		CultureInfo _defaultUICulture;

		public BaseTestFixture()
		{
			_defaultCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_defaultUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
			Device.PlatformServices = new MockPlatformServices();
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
				Device.PlatformServices = null;
				System.Threading.Thread.CurrentThread.CurrentCulture = _defaultCulture;
				System.Threading.Thread.CurrentThread.CurrentUICulture = _defaultUICulture;
			}

			_disposed = true;
		}
	}
}
