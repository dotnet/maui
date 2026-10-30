using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// The backend's layout container: a custom <see cref="global::Gtk.Widget"/> (not <c>Box</c>/<c>Fixed</c>, whose layout
	/// managers would bypass the virtual methods) that delegates measuring and arranging to MAUI's
	/// cross-platform layout (<see cref="ICrossPlatformLayoutBacking"/>).
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Measure: <c>OnMeasure</c> runs <see cref="ICrossPlatformLayout.CrossPlatformMeasure"/>; MAUI infinity is GTK <c>-1</c>.</item>
	/// <item>Arrange: <c>OnSizeAllocate</c> runs <see cref="ICrossPlatformLayout.CrossPlatformArrange"/>; each child's handler allocates
	/// it from inside this call (see <see cref="GtkPlatformViewBackend"/>). There is no <c>SetSizeRequest</c> and no write-back.</item>
	/// <item>The minimum size is 0 (layouts may be smaller than their content, as on the other platforms); the natural size is the desired size.</item>
	/// </list>
	/// </remarks>
	public class MauiGtkPanel : global::Gtk.Widget, ICrossPlatformLayoutBacking, IGtkChildHost
	{
		/// <inheritdoc/>
		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

		/// <summary>Whether the panel clips its children to its allocation.</summary>
		public bool ClipsToBounds
		{
			get => Overflow == global::Gtk.Overflow.Hidden;
			set => Overflow = value ? global::Gtk.Overflow.Hidden : global::Gtk.Overflow.Visible;
		}

		/// <inheritdoc/>
		protected override global::Gtk.SizeRequestMode OnGetRequestMode() => global::Gtk.SizeRequestMode.HeightForWidth;

		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			minimumBaseline = naturalBaseline = -1;
			minimum = 0;

			var layout = CrossPlatformLayout;
			if (layout is null)
			{
				natural = 0;
				return;
			}

			double widthConstraint = double.PositiveInfinity;
			double heightConstraint = double.PositiveInfinity;

			// HeightForWidth: the horizontal pass is unconstrained, the vertical pass carries the width.
			if (orientation == global::Gtk.Orientation.Vertical && forSize >= 0)
				widthConstraint = forSize;
			else if (orientation == global::Gtk.Orientation.Horizontal && forSize >= 0)
				heightConstraint = forSize;

			var size = layout.CrossPlatformMeasure(widthConstraint, heightConstraint);
			natural = GtkInterop.ToPixels(orientation == global::Gtk.Orientation.Horizontal ? size.Width : size.Height);
		}

		/// <inheritdoc/>
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			ArrangeChildren(width, height);
		}

		/// <summary>Runs the cross-platform arrange for the given allocation. Overridden by the root panel, which must measure first.</summary>
		/// <param name="width">The allocated width.</param>
		/// <param name="height">The allocated height.</param>
		protected virtual void ArrangeChildren(int width, int height)
		{
			CrossPlatformLayout?.CrossPlatformArrange(new Rect(0, 0, width, height));
		}

		/// <summary>Inserts <paramref name="child"/> at <paramref name="index"/> (clamped); the sibling order is the z-order.</summary>
		/// <param name="index">The target index.</param>
		/// <param name="child">The child widget.</param>
		public void InsertChild(int index, global::Gtk.Widget child)
		{
			if (child is null)
				throw new ArgumentNullException(nameof(child));

			if (child.Parent is not null)
			{
				if (ReferenceEquals(child.Parent, this))
					child.Unparent();
				else
					throw new InvalidOperationException("The child already belongs to another parent.");
			}

			global::Gtk.Widget? before = FirstChild;
			for (int i = 0; i < index && before is not null; i++)
				before = before.NextSibling;

			if (before is null)
				child.Parent = this;
			else
				child.InsertBefore(this, before);

			QueueResize();
		}

		/// <summary>Appends <paramref name="child"/>.</summary>
		/// <param name="child">The child widget.</param>
		public void AddChild(global::Gtk.Widget child) => InsertChild(int.MaxValue, child);

		/// <summary>Removes <paramref name="child"/> if it is a child of this panel.</summary>
		/// <param name="child">The child widget.</param>
		public void RemoveChild(global::Gtk.Widget child)
		{
			if (child is not null && ReferenceEquals(child.Parent, this))
			{
				child.Unparent();
				QueueResize();
			}
		}

		/// <summary>The index of <paramref name="child"/>, or -1.</summary>
		/// <param name="child">The child widget.</param>
		public int IndexOfChild(global::Gtk.Widget child)
		{
			int index = 0;
			for (var current = FirstChild; current is not null; current = current.NextSibling, index++)
			{
				if (ReferenceEquals(current, child))
					return index;
			}

			return -1;
		}

		/// <summary>Removes every child.</summary>
		public void ClearChildren()
		{
			global::Gtk.Widget? child;
			while ((child = FirstChild) is not null)
				child.Unparent();

			QueueResize();
		}

		/// <inheritdoc/>
		public void ReplaceChild(global::Gtk.Widget oldChild, global::Gtk.Widget newChild)
		{
			int index = IndexOfChild(oldChild);
			if (index < 0)
				return;

			oldChild.Unparent();
			InsertChild(index, newChild);
		}
	}

	/// <summary>
	/// The panel that hosts a window's content. It asks for no size of its own (the window decides) and runs the
	/// cross-platform measure itself before arranging, because nothing above it measures with the final size.
	/// </summary>
	public sealed class MauiGtkRootPanel : MauiGtkPanel
	{
		/// <inheritdoc/>
		protected override void OnMeasure(global::Gtk.Orientation orientation, int forSize,
			out int minimum, out int natural, out int minimumBaseline, out int naturalBaseline)
		{
			// The window decides the size; asking for the content size here is what made XF windows run away.
			minimum = natural = 0;
			minimumBaseline = naturalBaseline = -1;
		}

		/// <summary>Raised from the size-allocate with the new size (before the content is arranged).</summary>
		public event Action<int, int>? SizeChanged;

		/// <inheritdoc/>
		protected override void ArrangeChildren(int width, int height)
		{
			SizeChanged?.Invoke(width, height);

			var layout = CrossPlatformLayout;
			if (layout is null)
				return;

			layout.CrossPlatformMeasure(width, height);
			layout.CrossPlatformArrange(new Rect(0, 0, width, height));
		}
	}
}
