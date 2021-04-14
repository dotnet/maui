using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ScrollViewStub : StubBase, IScroll
	{
		public IView Content { get; set; }

		public SizeF ContentSize { get; set; }

		public ScrollOrientation Orientation { get; set; }

		public ScrollBarVisibility HorizontalScrollBarVisibility { get; set; }

		public ScrollBarVisibility VerticalScrollBarVisibility { get; set; }
	}
}