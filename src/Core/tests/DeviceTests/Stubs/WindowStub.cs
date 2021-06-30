namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : StubBase, IWindow
	{
		public IView Content { get; set; }

		public string Title { get; set; }
	}
}