using System;
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

		/// <summary>
		/// Occurs when a hover interaction starts on the surface.
		/// </summary>
		/// <param name="points">Points for the hover interaction location.</param>
		void StartHoverInteraction(PointF[] points);

		/// <summary>
		/// Occurs when a hover interaction moves on the surface.
		/// </summary>
		/// <param name="points">Points of the interaction location.</param>
		void HoverInteraction(PointF[] points);

		/// <summary>
		/// Occurs when a hover interaction ends on the surface.
		/// </summary>
		/// <param name="points">Points of the interaction location.</param>
		void EndHoverInteraction();

		/// <summary>
		/// Occurs when a touch or click interaction starts on the surface.
		/// </summary>
		/// <param name="points">Points of the interaction location.</param>
		void StartInteraction(PointF[] points);

		/// <summary>
		/// Occurs when a touch or click interaction moves (drags) on the surface.
		/// </summary>
		/// <param name="points">Points of the interaction location.</param>
		void DragInteraction(PointF[] points);

		/// <summary>
		/// Occurs when a touch or click interaction ends on the surface.
		/// </summary>
		/// <param name="points">Points of the interaction location.</param>
		/// <param name="inside">Specifies whether or not the interaction ended inside the surface.</param>
		void EndInteraction(PointF[] points, bool inside);

		/// <summary>
		/// Occurs when a touch or click interaction ends on the surface.
		/// </summary>
		void CancelInteraction();
	}
}