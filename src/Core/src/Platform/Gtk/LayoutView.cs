#define TRACE_ALLOCATION

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics.Native.Gtk;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;
using Point = Microsoft.Maui.Graphics.Point;

namespace Microsoft.Maui.Native
{

	// refactored from: https://github.com/mono/xwt/blob/501f6b529fca632655295169094f637627c74c47/Xwt.Gtk/Xwt.GtkBackend/BoxBackend.cs

	public class LayoutView : Container, IGtkContainer
	{

		protected override bool OnDrawn(Cairo.Context cr)
		{
			var stc = this.StyleContext;
			stc.RenderBackground(cr, 0, 0, Allocation.Width, Allocation.Height);

			var r = base.OnDrawn(cr);
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

		public Func<ILayout>? CrossPlatformVirtualView { get; set; }

		public ILayout? VirtualView => CrossPlatformVirtualView?.Invoke();

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
			if (VirtualView is not { LayoutManager: { } layoutManager } virtualView)
				return;

			layoutManager.ArrangeChildren(allocation);

		}

		protected bool RestrictToMesuredAllocation { get; set; } = true;

		protected bool RestrictToMeasuredArrange { get; set; } = true;

		protected bool IsReallocating;

		protected bool IsSizeAllocating;

		protected Size? MeasuredSizeH { get; set; }

		protected Size? MeasuredSizeV { get; set; }

		protected Size? MeasuredMinimum { get; set; }

		protected Rectangle LastAllocation { get; set; }

		protected void ClearMeasured(bool clearCache = true)
		{
			if (clearCache && !MeasureCache.IsEmpty)
			{
				MeasureCache.Clear();
			}

			MeasuredSizeH = null;
			MeasuredSizeV = null;
			MeasuredMinimum = null;

		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{

			if (IsSizeAllocating)
				return;

			if (VirtualView is not { } virtualView)
			{
				base.OnSizeAllocated(allocation);

				return;
			}

			var clearCache = true;

			try
			{
				IsReallocating = true;

				var mAllocation = allocation.ToRectangle();

				clearCache = LastAllocation.IsEmpty || mAllocation.IsEmpty || LastAllocation != mAllocation;
				ClearMeasured(clearCache);

				LastAllocation = mAllocation;

				var mesuredAllocation = Measure(allocation.Width, allocation.Height);

				if (RestrictToMesuredAllocation)
					mAllocation.Size = mesuredAllocation;

				ArrangeAllocation(new Rectangle(Point.Zero, mAllocation.Size));
				AllocateChildren(mAllocation);

				if (virtualView.Frame != mAllocation)
				{
					IsSizeAllocating = true;

					Arrange(mAllocation);
				}

				base.OnSizeAllocated(allocation);

			}
			finally
			{
				IsReallocating = false;
				IsSizeAllocating = false;
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
					LastAllocation = Allocation.ToRectangle();
					Measure(Allocation.Width, Allocation.Height);

				}
				catch
				{
					IsReallocating = false;
				}
			}

			base.OnRealized();
		}

#if TRACE_ALLOCATION
		int _measureCount = 0;
		bool _checkCacheHitFailed = false;
#endif

		protected ConcurrentDictionary<(double width, double height, SizeRequestMode mode), Size> MeasureCache { get; } = new();

		public Size Measure(double widthConstraint, double heightConstraint, SizeRequestMode mode = SizeRequestMode.ConstantSize)
		{

			bool CanBeCached() => !double.IsPositiveInfinity(widthConstraint) && !double.IsPositiveInfinity(heightConstraint);

			if (VirtualView is not { LayoutManager: { } layoutManager } virtualView)
				return Size.Zero;

			var key = (widthConstraint, heightConstraint, mode);

			Size cached = Size.Zero;

			bool cacheHit = CanBeCached() && MeasureCache.TryGetValue(key, out cached);

			if (cacheHit)
			{
#if TRACE_ALLOCATION
				if (!_checkCacheHitFailed)
#endif
					return cached;

			}

			var measured = layoutManager.Measure(widthConstraint, heightConstraint);

#if TRACE_ALLOCATION
			if (_checkCacheHitFailed && cacheHit && measured != cached)
			{
				Debug.WriteLine($"{cached} =! {measured}");
			}

			_measureCount++;
#endif

			if (CanBeCached())
				MeasureCache[key] = measured;

			return measured;
		}

		protected Size MeasureMinimum()
		{
			if (MeasuredMinimum != null)
				return MeasuredMinimum.Value;

			if (VirtualView is not { } virtualView)
				return Size.Zero;

			// ensure all children have DesiredSize:

			Measure(0, double.PositiveInfinity);

			var desiredMinimum = virtualView.Aggregate(new Size(), (s, c) => new Size(Math.Max(s.Width, c.DesiredSize.Width), s.Height + c.DesiredSize.Height));

			MeasuredMinimum = Measure(desiredMinimum.Width, double.PositiveInfinity);

			return MeasuredMinimum.Value;
		}

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
		{
			base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

			if (IsSizeAllocating)
			{
				return;
			}

			if (VirtualView is not { LayoutManager: { } layoutManager } virtualView)
				return;

			var measuredMinimum = MeasureMinimum();

			double constraint = minimumSize;

			if (orientation == Orientation.Horizontal)
			{
				if (RequestMode is SizeRequestMode.WidthForHeight or SizeRequestMode.ConstantSize)
				{
					if (MeasuredSizeV is { Width : > 0 } size && (constraint == 0))
						constraint = size.Width;

					constraint = constraint == 0 ? double.PositiveInfinity : constraint;
				}
				else
				{
					;
				}

				MeasuredSizeH = constraint != 0 ? Measure(constraint, double.PositiveInfinity) : measuredMinimum;

				constraint = MeasuredSizeH.Value.Width;

				minimumSize = (int)measuredMinimum.Width;
				naturalSize = (int)constraint;
			}

			if (orientation == Orientation.Vertical)
			{
				var widthContraint = double.PositiveInfinity;

				if (RequestMode is SizeRequestMode.HeightForWidth or SizeRequestMode.ConstantSize)
				{
					MeasuredSizeH ??= measuredMinimum;

					if (MeasuredSizeH is { } size && constraint == 0)
					{
						if (size.Height > 0)
							constraint = size.Height;

						if (size.Width > 0)
							widthContraint = size.Width;
					}

					constraint = constraint == 0 ? double.PositiveInfinity : constraint;

				}
				else
				{
					;
				}

				MeasuredSizeV = constraint != 0 ? Measure(widthContraint, constraint) : measuredMinimum;

				constraint = MeasuredSizeV.Value.Height;

				minimumSize = (int)measuredMinimum.Height;
				naturalSize = (int)constraint;

			}
		}

		public void Arrange(Rectangle rect)
		{

			if (rect.IsEmpty)
				return;

			if (rect == Allocation.ToRectangle()) return;

			if (IsSizeAllocating)
			{

				SizeAllocate(rect.ToNative());

				return;
			}

			var measuredArrange = Measure(rect.Width, rect.Height);
			var alloc = new Rectangle(rect.Location, RestrictToMeasuredArrange ? measuredArrange : rect.Size);
			SizeAllocate(alloc.ToNative());
			QueueAllocate();
		}

		protected int ToSize(double it) => double.IsPositiveInfinity(it) ? 0 : (int)it;

	}

}