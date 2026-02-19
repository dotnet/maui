#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	[ElementHandler(typeof(GraphicsViewHandler))]
	public class GraphicsView : View, IGraphicsView
	{
		public event EventHandler<TouchEventArgs> StartHoverInteraction;
		public event EventHandler<TouchEventArgs> MoveHoverInteraction;
		public event EventHandler EndHoverInteraction;
		public event EventHandler<TouchEventArgs> StartInteraction;
		public event EventHandler<TouchEventArgs> DragInteraction;
		public event EventHandler<TouchEventArgs> EndInteraction;
		public event EventHandler CancelInteraction;

		/// <summary>Bindable property for <see cref="Drawable"/>.</summary>
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

		void IGraphicsView.CancelInteraction() => CancelInteraction?.Invoke(this, EventArgs.Empty);

		void IGraphicsView.DragInteraction(PointF[] points) => DragInteraction?.Invoke(this, new TouchEventArgs(points, true));

		void IGraphicsView.EndHoverInteraction() => EndHoverInteraction?.Invoke(this, EventArgs.Empty);

		void IGraphicsView.EndInteraction(PointF[] points, bool isInsideBounds) => EndInteraction?.Invoke(this, new TouchEventArgs(points, isInsideBounds));

		void IGraphicsView.StartHoverInteraction(PointF[] points) => StartHoverInteraction?.Invoke(this, new TouchEventArgs(points, true));

		void IGraphicsView.MoveHoverInteraction(PointF[] points) => MoveHoverInteraction?.Invoke(this, new TouchEventArgs(points, true));

		void IGraphicsView.StartInteraction(PointF[] points) => StartInteraction?.Invoke(this, new TouchEventArgs(points, true));
	}
	public class TouchEventArgs : EventArgs
	{
		public TouchEventArgs()
		{

		}

		public TouchEventArgs(PointF[] points, bool isInsideBounds)
		{
			Touches = points;
			IsInsideBounds = isInsideBounds;
		}

		/// <summary>
		/// This is only used for EndInteraction;
		/// </summary>
		public bool IsInsideBounds { get; private set; }

		public PointF[] Touches { get; private set; }
	}

}