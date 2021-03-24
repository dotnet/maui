namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
	}
}