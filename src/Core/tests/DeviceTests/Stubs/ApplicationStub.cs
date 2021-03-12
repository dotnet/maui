using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ApplicationStub : Application, IDisposable
	{
		public override IWindow CreateWindow(IActivationState state)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Current = null;
		}
	}
}