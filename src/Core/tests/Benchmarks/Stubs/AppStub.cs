namespace Microsoft.Maui.Handlers.Benchmarks
{
	class AppStub : MauiApp
	{
		public override IWindow CreateWindow(IActivationState state)
		{
			return new WindowStub();
		}
	}
}