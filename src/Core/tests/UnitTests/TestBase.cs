using System;
using Microsoft.Maui.Dispatching;

namespace Microsoft.Maui.UnitTests
{
	public class TestBase : IDisposable
	{
		public TestBase()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
		}
	}
}
