using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Primitives;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// The GTK implementation of Core's platform-view backend hook: measures, arranges and wraps GTK widgets for every
	/// <c>ViewHandler</c>, mirroring what <c>ViewHandlerExtensions.GetDesiredSizeFromHandler</c> /
	/// <c>PlatformArrangeHandler</c> do on the other platforms.
	/// </summary>
	/// <remarks>
	/// GTK 4 rules honoured here: a widget is always measured before it is allocated, an allocation is never smaller than
	/// the widget's minimum, and a child is allocated only from inside its parent's size-allocate (the call chain
	/// <c>OnSizeAllocate -> CrossPlatformArrange -> PlatformArrange</c>).
	/// </remarks>
	internal sealed class GtkPlatformViewBackend : IPlatformViewBackend
	{
		/// <summary>The singleton installed into <see cref="PlatformViewBackend.Current"/>.</summary>
		public static GtkPlatformViewBackend Instance { get; } = new();

		/// <summary>Installs the backend. Idempotent.</summary>
		public static void Install() => PlatformViewBackend.Current = Instance;

		/// <summary>The widget MAUI should lay out for <paramref name="handler"/>: the container when there is one.</summary>
		public static global::Gtk.Widget? GetLayoutWidget(IViewHandler handler) =>
			(handler.ContainerView ?? handler.PlatformView) as global::Gtk.Widget;

		/// <inheritdoc/>
		public Size GetDesiredSize(IViewHandler handler, double widthConstraint, double heightConstraint)
		{
			var view = handler.VirtualView;
			var widget = GetLayoutWidget(handler);

			if (widget is null || view is null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			// Measure results are fed into children of the same MAUI measure pass, so honour the view's own maximums first.
			widthConstraint = Math.Min(widthConstraint, view.MaximumWidth);
			heightConstraint = Math.Min(heightConstraint, view.MaximumHeight);

			bool widthExplicit = IsExplicitSet(view.Width);
			bool heightExplicit = IsExplicitSet(view.Height);

			// GTK is height-for-width: settle the width first, then ask for the height at that width.
			widget.Measure(global::Gtk.Orientation.Horizontal, -1, out int minWidth, out int naturalWidth, out _, out _);

			double width;
			if (widthExplicit)
				width = view.Width;
			else
				width = Math.Max(minWidth, double.IsInfinity(widthConstraint) ? naturalWidth : Math.Min(naturalWidth, widthConstraint));

			widget.Measure(global::Gtk.Orientation.Vertical, GtkInterop.ToPixels(width), out int minHeight, out int naturalHeight, out _, out _);

			double height = heightExplicit ? view.Height : Math.Max(minHeight, naturalHeight);

			return new Size(
				ResolveConstraints(width, view.Width, view.MinimumWidth, view.MaximumWidth),
				ResolveConstraints(height, view.Height, view.MinimumHeight, view.MaximumHeight));
		}

		/// <inheritdoc/>
		public void PlatformArrange(IViewHandler handler, Rect frame)
		{
			var widget = GetLayoutWidget(handler);
			if (widget is null || frame.Width < 0 || frame.Height < 0)
				return;

			var state = GtkViewState.For(widget);
			state.Frame = frame;
			state.HasFrame = true;

			Allocate(widget, state);

			handler.Invoke(nameof(IView.Frame), frame);
		}

		/// <summary>
		/// Allocates <paramref name="widget"/> at its stored frame with its translation/scale/rotation applied.
		/// Callers outside the parent's allocate (a transform changed) use <see cref="Reallocate"/>.
		/// </summary>
		internal static void Allocate(global::Gtk.Widget widget, GtkViewState state)
		{
			if (!widget.Visible)
				return;

			var frame = state.Frame;
			int width = GtkInterop.ToPixels(frame.Width);
			int height = GtkInterop.ToPixels(frame.Height);

			// Measure first (GTK drops an allocation of a never-measured widget) and never allocate below the minimum.
			widget.Measure(global::Gtk.Orientation.Horizontal, -1, out int minWidth, out _, out _, out _);
			width = Math.Max(width, minWidth);
			widget.Measure(global::Gtk.Orientation.Vertical, width, out int minHeight, out _, out _, out _);
			height = Math.Max(height, minHeight);

			widget.Allocate(width, height, -1, CreateTransform(frame, state, width, height));
		}

		/// <summary>Re-allocates a widget at its last frame after a transform property changed.</summary>
		/// <param name="widget">The widget (container or platform view).</param>
		internal static void Reallocate(global::Gtk.Widget widget)
		{
			var state = GtkViewState.TryGet(widget);
			if (state is null || !state.HasFrame || widget.Parent is null)
				return;

			Allocate(widget, state);
			widget.Parent.QueueDraw();
		}

		static Gsk.Transform CreateTransform(Rect frame, GtkViewState state, int width, int height)
		{
			var transform = new Gsk.Transform()
				.Translate(GtkInterop.ToGraphene(frame.X + state.TranslationX, frame.Y + state.TranslationY));

			if (state.Rotation != 0 || state.ScaleX != 1 || state.ScaleY != 1)
			{
				double anchorX = state.AnchorX * width;
				double anchorY = state.AnchorY * height;

				transform = transform.Translate(GtkInterop.ToGraphene(anchorX, anchorY));
				if (state.Rotation != 0)
					transform = transform.Rotate((float)state.Rotation);
				if (state.ScaleX != 1 || state.ScaleY != 1)
					transform = transform.Scale((float)state.ScaleX, (float)state.ScaleY);
				transform = transform.Translate(GtkInterop.ToGraphene(-anchorX, -anchorY));
			}

			return transform;
		}

		/// <inheritdoc/>
		public void SetupContainer(IViewHandler handler)
		{
			if (handler is not ViewHandler viewHandler || handler.ContainerView is not null)
				return;

			if (handler.PlatformView is not global::Gtk.Widget platformView)
				return;

			var parent = platformView.Parent;
			var container = new GtkWrapperView();

			if (parent is IGtkChildHost host)
			{
				var state = GtkViewState.TryGet(platformView);
				host.ReplaceChild(platformView, container);
				container.Child = platformView;
				if (state is not null)
					GtkViewState.For(container).CopyFrom(state);
			}
			else
			{
				if (parent is not null)
					platformView.Unparent();
				container.Child = platformView;
			}

			viewHandler.SetContainerView(container);
		}

		/// <inheritdoc/>
		public void RemoveContainer(IViewHandler handler)
		{
			if (handler is not ViewHandler viewHandler || handler.ContainerView is not GtkWrapperView container)
				return;

			var platformView = handler.PlatformView as global::Gtk.Widget;
			var parent = container.Parent;

			container.Child = null;

			if (platformView is not null && parent is IGtkChildHost host)
			{
				var state = GtkViewState.TryGet(container);
				host.ReplaceChild(container, platformView);
				if (state is not null)
					GtkViewState.For(platformView).CopyFrom(state);
			}
			else if (parent is not null)
			{
				container.Unparent();
			}

			viewHandler.SetContainerView(null);
		}

		/// <summary>Mirrors <c>ViewHandlerExtensions.ResolveConstraints</c> of the other platforms.</summary>
		internal static double ResolveConstraints(double measured, double exact, double min, double max)
		{
			var resolved = measured;

			min = ResolveMinimum(min);

			if (IsExplicitSet(exact))
				resolved = exact;

			if (resolved > max)
				resolved = max;

			if (resolved < min)
				resolved = min;

			return resolved;
		}
	}
}
