namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : StubBase, IWindow
	{
		public IView View { get; set; }

		public string Id => throw new System.NotImplementedException();
	}
}