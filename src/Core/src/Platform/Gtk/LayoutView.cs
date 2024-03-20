#define TRACE_ALLOCATION_
#define USE_ADJUSTSIZEREQUEST_
using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Rectangle = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using Point = Microsoft.Maui.Graphics.Point;

#pragma warning disable CS0162 // Unreachable code detected

namespace Microsoft.Maui.Platform
{
	// refactored from: https://github.com/mono/xwt/blob/501f6b529fca632655295169094f637627c74c47/Xwt.Gtk/Xwt.GtkBackend/BoxBackend.cs

	// see: https://github.com/GNOME/gtk/blob/gtk-3-22/gtk/gtkcontainer.c

	public class LayoutView : Container, IGtkContainer, ICrossPlatformLayoutBacking
	{
		protected override bool OnDrawn(Cairo.Context cr)
		{
			var stc = this.StyleContext;
			stc.RenderBackground(cr, 0, 0, Allocation.Width, Allocation.Height);

			var r = true; 
			foreach (var c in _children.ToArray())
				PropagateDraw(c.widget, cr);
#if TRACE_ALLOCATION
			cr.Save();
			cr.SetSourceColor(Graphics.Colors.Red.ToCairoColor());
			cr.Rectangle(0, 0, Allocation.Width, Allocation.Height);
			cr.Stroke();

			cr.MoveTo(0, Allocation.Height - 12);
			cr.ShowText($"{_measureCount} | {Allocation.Size}");
			cr.Restore();
#endif
			return r;
		}

		public ICrossPlatformLayout? CrossPlatformLayout { get; set; }

		List<(IView view, Widget widget)> _children = new();

		public LayoutView()
		{
			HasWindow = false;
		}

		public void ReplaceChild(Widget oldWidget, Widget newWidget)
		{
			var index = _children.FindIndex(c => c.widget == oldWidget);

			if (index == -1)
				return;

			var view = _children[index].view;

			Remove(oldWidget);
			Add(newWidget);

			if (view != null)
			{
				_children[index] = (view, newWidget);
			}
		}

		// this is maybe not needed:
		void UpdateFocusChain()
		{
			Orientation GetOrientation() =>
				// TODO: find out what orientation it has, or find another sort kriteria, eg. tabstop
				Orientation.Vertical;

			var orientation = GetOrientation();

			var focusChain = _children
				.Select(c => c.widget)
				// .OrderBy(kvp => orientation == Orientation.Horizontal ? kvp.Value.Rect.X : kvp.Value.Rect.Y)
				.ToArray();

			FocusChain = focusChain;
		}

		protected override void ForAll(bool includeInternals, Callback callback)
		{
			base.ForAll(includeInternals, callback);

			foreach (var c in _children.ToArray())
				callback(c.widget);
		}

		public void ClearChildren()
		{
			ClearMeasured();

			foreach (var c in Children)
			{
				Remove(c);
			}

			_children.Clear();
		}

		public void Add(IView view, Widget widget)
		{
			var index = _children.FindIndex(c => c.widget == widget);

			if (index != -1)
				return;

			_children.Add((view, widget));

			Add(widget);
		}

		public void Insert(IView view, Widget widget, int index)
		{
			_children.Insert(index, (view, widget));
			Add(widget);
		}

		public void Update(IView view, Widget widget, int index)
		{
			var replace = _children[index];
			_children[index] = (view, widget);
			Remove(replace.widget);
			Add(widget);
		}

		protected override void OnAdded(Widget widget)
		{
			widget.Parent = this;
			ClearMeasured();
		}

		protected override void OnRemoved(Widget widget)
		{
			widget.Unparent();
			ClearMeasured();
			QueueResize();
		}

		protected void AllocateChildren(Rectangle allocation)
		{
			foreach (var cr in _children.ToArray())
			{
				var w = cr.widget;
				var v = cr.view;
				var r = v.Frame;

				if (r.IsEmpty)
					continue;

				var cAlloc = new Gdk.Rectangle((int)(allocation.X + r.X), (int)(allocation.Y + r.Y), (int)r.Width, (int)r.Height);

				// if (cAlloc != w.Allocation) // it's allways needed to implicit arrange children:
				w.SizeAllocate(cAlloc);
			}
		}

		protected void ArrangeAllocation(Rectangle allocation)
		{
			if (CrossPlatformLayout is not { } virtualView)
			{
				return;
			}

			virtualView.CrossPlatformArrange(allocation);
		}

		protected bool RestrictToMeasuredAllocation { get; set; } = false;

		protected bool RestrictToMeasuredArrange { get; set; } = false;

		protected bool IsReallocating;

		protected bool IsSizeAllocating;

		protected Size? MeasuredSizeH { get; set; }

		protected Size? MeasuredSizeV { get; set; }

		protected Rectangle LastAllocation { get; set; }

		protected Rectangle? CurrentAllocation { get; set; }

		protected void ClearMeasured(bool clearCache = true)
		{
			if (clearCache)
			{
				MeasuredSizeH = null;
				MeasuredSizeV = null;
			}

#if !USE_ADJUSTSIZEREQUEST
			RequestedWidth = null;
			RequestedHeight = null;

#endif
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			if (IsSizeAllocating)
				return;

			if (CrossPlatformLayout is not { } crossPlatformLayout)
			{
				base.OnSizeAllocated(allocation);

				return;
			}

			var clearCache = true;

			try
			{
				IsReallocating = true;

				var mAllocation = allocation.ToRect();
				CurrentAllocation = mAllocation;

				clearCache = LastAllocation.IsEmpty || mAllocation.IsEmpty || LastAllocation != mAllocation;
				ClearMeasured(clearCache);

				if (RestrictToMeasuredAllocation)
				{
					var mesuredAllocation = Measure(allocation.Width, allocation.Height);
					mAllocation.Size = mesuredAllocation;
					CurrentAllocation = mAllocation;
				}

				ArrangeAllocation(new Rectangle(Point.Zero, mAllocation.Size));
				AllocateChildren(mAllocation);

				if (crossPlatformLayout is IView virualView && virualView.Frame != mAllocation)
				{
					// IsSizeAllocating = true;

					// Arrange(mAllocation);
				}

				base.OnSizeAllocated(allocation);
				LastAllocation = mAllocation;
			}
			finally
			{
				IsReallocating = false;
				IsSizeAllocating = false;
				CurrentAllocation = null;
			}
		}

		protected override void OnUnrealized()
		{
			// force reallocation on next realization, since allocation may be lost
			IsReallocating = false;
			ClearMeasured();
			base.OnUnrealized();
		}

		protected override void OnRealized()
		{
			// force reallocation, if unrealized previously
			if (!IsReallocating)
			{
				try
				{
					LastAllocation = Allocation.ToRect();
					CurrentAllocation = LastAllocation;
					Measure(Allocation.Width, Allocation.Height);
				}
				catch
				{
					CurrentAllocation = null;
					IsReallocating = false;
				}
			}

			base.OnRealized();
		}

#if TRACE_ALLOCATION
		int _measureCount = 0;
#endif

		public Size Measure(double widthConstraint, double heightConstraint, SizeRequestMode mode = SizeRequestMode.ConstantSize)
		{
			if (CrossPlatformLayout is not { } virtualView)
			{
				return Size.Zero;
			}

			var measured = virtualView.CrossPlatformMeasure(widthConstraint, heightConstraint);

#if TRACE_ALLOCATION
			_measureCount++;
#endif

			return measured;
		}

#if !USE_ADJUSTSIZEREQUEST

		// protected override SizeRequestMode OnGetRequestMode()
		// {
		// 	// dirty fix: unwrapped labels report fixed sizes, forcing parents to fixed mode
		// 	//            -> report always width_for_height, since we don't support angles
		// 	return Gtk.SizeRequestMode.WidthForHeight;
		// }

		double? RequestedHeight { get; set; }

		double? RequestedWidth { get; set; }

		protected override void OnGetPreferredHeight(out int minimum_height, out int natural_height)
		{
			base.OnGetPreferredHeight(out minimum_height, out natural_height);
			// containers need initial width in height_for_width mode
			// dirty fix: do not constrain width on first allocation 
			var force_width = RequestedWidth ?? double.PositiveInfinity;

			if (IsReallocating && CurrentAllocation.HasValue)
				force_width = CurrentAllocation.Value.Width;

			var size = Measure(force_width, minimum_height > 0 ? minimum_height : RequestedHeight ?? double.PositiveInfinity);

			if (size.Height < HeightRequest)
				minimum_height = natural_height = HeightRequest;
			else
				minimum_height = natural_height = (int)size.Height;
		}

		protected override void OnGetPreferredWidth(out int minimum_width, out int natural_width)
		{
			base.OnGetPreferredWidth(out minimum_width, out natural_width);
			// containers need initial height in width_for_height mode
			// dirty fix: do not constrain height on first allocation
			var force_height = RequestedHeight ?? double.PositiveInfinity;

			if (IsReallocating && CurrentAllocation.HasValue)
				force_height = CurrentAllocation.Value.Height;

			var size = Measure(minimum_width > 0 ? minimum_width : RequestedWidth ?? double.PositiveInfinity, force_height);

			if (size.Width < WidthRequest)
				minimum_width = natural_width = WidthRequest;
			else
				minimum_width = natural_width = (int)size.Width;
		}

		protected override void OnGetPreferredHeightForWidth(int width, out int minimum_height, out int natural_height)
		{
			RequestedWidth = width;

			base.OnGetPreferredHeightForWidth(width, out minimum_height, out natural_height);

			var size = Measure(width, minimum_height > 0 ? minimum_height : RequestedHeight ?? double.PositiveInfinity);

			if (size.Height < HeightRequest)
				minimum_height = natural_height = HeightRequest;
			else
				minimum_height = natural_height = (int)size.Height;

			RequestedWidth = null;
		}

		protected override void OnGetPreferredWidthForHeight(int height, out int minimum_width, out int natural_width)
		{
			RequestedHeight = height;

			base.OnGetPreferredWidthForHeight(height, out minimum_width, out natural_width);
			var size = Measure(minimum_width > 0 ? minimum_width : RequestedWidth ?? double.PositiveInfinity, height);

			if (size.Width < WidthRequest)
				minimum_width = natural_width = WidthRequest;
			else
				minimum_width = natural_width = (int)size.Width;

			RequestedHeight = null;
		}

#endif

#if USE_ADJUSTSIZEREQUEST
		// see: https://github.com/GNOME/gtk/blob/gtk-3-22/gtk/gtksizerequest.c#L362

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
		{
			base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

			if (IsSizeAllocating)
			{
				return;
			}

			if (CrossPlatformLayout is not { } virtualView)
				return;

			double allocation = orientation == Orientation.Vertical ? AllocatedWidth : AllocatedHeight;

			double hConstraint = IsReallocating ? AllocatedHeight : double.PositiveInfinity;
			double wConstraint = IsReallocating ? AllocatedWidth : 0;

			var requestMode = RequestMode;

			if (requestMode != SizeRequestMode.HeightForWidth)
			{
				;
			}

			if (orientation == Orientation.Horizontal)
			{
				// constraint = constraint == 0 && MeasuredSizeV is { } size ? size.Height : constraint;
				if (minimumSize > 0)
					wConstraint = minimumSize;
				var size = new Size(wConstraint, hConstraint);
				var measuredMinimum = Measure(size.Width, size.Height);
				MeasuredSizeH = measuredMinimum;
				minimumSize = (int)measuredMinimum.Width;
				naturalSize = (int)minimumSize;
			}

			if (orientation == Orientation.Vertical)
			{
				// constraint = constraint == 0 && MeasuredSizeH is { } hsize ? hsize.Width : constraint;
				if (minimumSize > 0)
					hConstraint = minimumSize;
				var size = new Size(wConstraint, hConstraint);
				var measuredMinimum = Measure(size.Width, size.Height);
				MeasuredSizeV = measuredMinimum;
				minimumSize = (int)measuredMinimum.Height;
				naturalSize = (int)minimumSize;
			}
		}
#endif

		public void Arrange(Rectangle rect)
		{
			if (rect.IsEmpty)
				return;

			if (rect == Allocation.ToRect()) return;

			if (IsSizeAllocating)
			{
				SizeAllocate(rect.ToNative());

				return;
			}

			var size = rect.Size;

			if (RestrictToMeasuredArrange)
			{
				var measuredArrange = Measure(rect.Width, rect.Height);
				size = measuredArrange;
			}

			var alloc = new Rectangle(rect.Location, size);
			SizeAllocate(alloc.ToNative());
			QueueAllocate();
		}

		public Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (CrossPlatformLayout is not IView virtualView)
				return new Size(widthConstraint, heightConstraint);

			double? explicitWidth = (virtualView.Width >= 0) ? virtualView.Width : null;
			double? explicitHeight = (virtualView.Height >= 0) ? virtualView.Height : null;

			var measuredSize = Measure(explicitWidth ?? widthConstraint, explicitHeight ?? heightConstraint);

			// apply width and height constraints if necessary
			// var desiredWidth = Math.Min(measuredSize.Width, widthConstraint);
			// var desiredHeight = Math.Min(measuredSize.Height, heightConstraint);

			// return new Size(desiredWidth, desiredHeight);
			return measuredSize;
		}

		protected int ToSize(double it) => double.IsPositiveInfinity(it) ? 0 : (int)it;
	}
}