using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class AppStub : MauiApp, IDisposable
	{
		public override IWindow CreateWindow(IActivationState state)
		{
			return new WindowStub();
		}

		public void Dispose()
		{
			Current = null;
		}
	}
}