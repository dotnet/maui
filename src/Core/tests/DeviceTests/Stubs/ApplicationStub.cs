namespace Microsoft.Maui.DeviceTests.Stubs
{
	class ApplicationStub : IApplication
	{
		public IWindow CreateWindow(IActivationState activationState)
		{
			return new WindowStub();
		}
	}
}