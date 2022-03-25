using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class GraphicsViewStub : StubBase, IGraphicsView
	{
		public IDrawable Drawable { get; set; }

		public void CancelInteraction() { }

		public void DragInteraction(PointF[] points) { }

		public void EndHoverInteraction() { }

		public void EndInteraction(PointF[] points, bool isInsideBounds) { }

		public void Invalidate() { }

		public void StartHoverInteraction(PointF[] points) { }

		public void MoveHoverInteraction(PointF[] points) { }

		public void StartInteraction(PointF[] points) { }
	}
}