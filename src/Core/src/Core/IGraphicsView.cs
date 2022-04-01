using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a view that can be drawn on using drawing commands.
	/// </summary>
	public interface IGraphicsView : IView
	{
		/// <summary>
		/// Define the drawing content.
		/// </summary>
		IDrawable Drawable { get; }

		/// <summary>
		/// Informs the canvas that it needs to redraw itself.
		/// </summary>
		void Invalidate();

		void StartHoverInteraction(PointF[] points);
		void MoveHoverInteraction(PointF[] points);
		void EndHoverInteraction();
		void StartInteraction(PointF[] points);
		void DragInteraction(PointF[] points);
		void EndInteraction(PointF[] points, bool isInsideBounds);
		void CancelInteraction();
	}
}