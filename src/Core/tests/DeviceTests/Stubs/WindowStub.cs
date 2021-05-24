namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : StubBase, IWindow
	{
		public IPage View { get; set; }
	}
}