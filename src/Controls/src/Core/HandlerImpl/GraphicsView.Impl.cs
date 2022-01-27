using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public class GraphicsView : View, IGraphicsView
	{
		public static readonly BindableProperty DrawableProperty =
			BindableProperty.Create(nameof(Drawable), typeof(IDrawable), typeof(GraphicsView), null);

		public IDrawable Drawable
		{
			set { SetValue(DrawableProperty, value); }
			get { return (IDrawable)GetValue(DrawableProperty); }
		}

		public void Invalidate()
		{
			Handler?.Invoke(nameof(IGraphicsView.Invalidate));
		}

		public virtual void StartHoverInteraction(PointF[] points)
		{
		}

		public virtual void HoverInteraction(PointF[] points)
		{
		}

		public virtual void EndHoverInteraction()
		{
		}

		public virtual void StartInteraction(PointF[] points)
		{
		}

		public virtual void DragInteraction(PointF[] points)
		{
		}

		public virtual void EndInteraction(PointF[] points, bool inside)
		{
		}

		public virtual void CancelInteraction()
		{
		}

	}
}