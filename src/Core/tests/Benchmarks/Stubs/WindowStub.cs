namespace Microsoft.Maui.Handlers.Benchmarks
{
	class WindowStub : IWindow
	{
		public IPage Page { get; set; }
		public IMauiContext MauiContext { get; set; }
	}
}
