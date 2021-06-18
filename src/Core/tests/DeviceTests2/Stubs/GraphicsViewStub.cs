using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class GraphicsViewStub : StubBase, IGraphicsView
	{
		public IDrawable Drawable { get; set; }
	}
}