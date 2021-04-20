using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class FrameStub : StubBase, IFrame
	{
		public IView Content { get; set; }

		public bool HasShadow { get; set; }

		public Color BorderColor { get; set; }

		public float CornerRadius { get; set; }
	}
}