using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ApplicationStub : IApplication
	{
		public IReadOnlyList<IWindow> Windows => throw new System.NotImplementedException();

		public IWindow CreateWindow(IActivationState activationState)
		{
			return new WindowStub();
		}
	}
}