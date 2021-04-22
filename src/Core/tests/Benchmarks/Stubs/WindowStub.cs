namespace Microsoft.Maui.Handlers.Benchmarks
{
	public class WindowStub : StubBase, IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
		public string Title { get; set; }
	}
}