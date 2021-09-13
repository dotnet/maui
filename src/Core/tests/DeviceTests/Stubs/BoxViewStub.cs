using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class BoxViewStub : StubBase, IBoxView
	{
		public Color Color { get; set; }

		public CornerRadius CornerRadius { get; set; }
	}
}
