using Microsoft.Maui.Graphics;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : StubBase, IWindow
	{
		public IMauiContext MauiContext { get; set; }
		public IPage Page { get; set; }
		public string Title { get; set; }
	}
}