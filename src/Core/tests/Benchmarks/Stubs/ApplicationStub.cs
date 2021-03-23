namespace Microsoft.Maui.Handlers.Benchmarks
{
	class ApplicationStub : IApplication
	{
		public IWindow CreateWindow(IActivationState state)
		{
			return new WindowStub();
		}
	}
}