using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics.Native.Gtk;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;

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
#if DEBUG

			cr.Save();
			cr.SetSourceColor(Graphics.Colors.Red.ToCairoColor());
			cr.Rectangle(0, 0, Allocation.Width, Allocation.Height);
			cr.Stroke();

			cr.Restore();

			return r;
		}
#endif

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
		}

		protected override void OnRemoved(Widget widget)
		{

			var view = _children.FirstOrDefault(kvp => kvp.Value == widget).Key;

			if (view != null)
				_children.Remove(view);

			widget.Unparent();
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

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			if (VirtualView is not { })
				return;

			try
			{
				IsReallocating = true;

				MesuredAllocation = MeasuredArrange ?? Measure(allocation.Width, allocation.Height, SizeRequestMode.ConstantSize);

				var mAllocation = allocation.ToRectangle();

				if (RestrictToMesuredAllocation)
					mAllocation.Size = MesuredAllocation.Value;

				ArrangeAllocation(mAllocation);
				AllocateChildren(mAllocation);
			}
			finally
			{
				IsReallocating = false;
			}

		}

		protected override void OnUnrealized()
		{
			// force reallocation on next realization, since allocation may be lost
			IsReallocating = false;
			base.OnUnrealized();
		}

		protected override void OnRealized()
		{
			// force reallocation, if unrealized previously
			if (!IsReallocating)
			{
				try
				{
					MesuredAllocation ??= Measure(Allocation.Width, Allocation.Height, SizeRequestMode.ConstantSize);
				}
				catch
				{
					IsReallocating = false;
				}
			}

			base.OnRealized();
		}

		int sr = 0;

		public SizeRequest Measure(double widthConstraint, double heightConstraint, SizeRequestMode mode)
		{

			if (VirtualView is not { LayoutManager: { } layoutManager } virtualView)
				return Size.Zero;

			var size1 = layoutManager.Measure(widthConstraint, heightConstraint);
			sr++;

			return new SizeRequest(size1, size1);
		}

		int ToSize(double it) => double.IsPositiveInfinity(it) ? 0 : (int)it;

		protected override void OnGetPreferredHeight(out int minimumHeight, out int naturalHeight)
		{
			SizeRequest size;

			if (MeasuredArrange.HasValue)
				size = MeasuredArrange.Value;
			else
			{

				if (RequestMode == SizeRequestMode.HeightForWidth)
				{
					OnGetPreferredWidth(out var minimumWidth, out var naturalWidth);
					size = Measure(minimumWidth, double.PositiveInfinity, SizeRequestMode.HeightForWidth);
				}
				else
				{
					size = Measure(double.PositiveInfinity, 0, SizeRequestMode.WidthForHeight);
				}
			}

			minimumHeight = Math.Max(HeightRequest, ToSize(size.Minimum.Height));
			naturalHeight = Math.Max(HeightRequest, ToSize(size.Request.Height));
		}

		protected override void OnGetPreferredWidth(out int minimumWidth, out int naturalWidth)
		{
			SizeRequest size;

			if (MeasuredArrange.HasValue)
				size = MeasuredArrange.Value;
			else
			{

				if (RequestMode == SizeRequestMode.HeightForWidth)
				{
					size = Measure(0, double.PositiveInfinity, SizeRequestMode.HeightForWidth);
				}
				else
				{
					GetPreferredHeight(out var minimumHeight, out var naturalHeight);
					size = Measure(0, minimumHeight, SizeRequestMode.WidthForHeight);
				}
			}

			minimumWidth = Math.Max(WidthRequest, ToSize(size.Minimum.Width));
			naturalWidth = Math.Max(WidthRequest, ToSize(size.Request.Width));

		}

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
		{
			base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

			if (!MeasuredArrange.HasValue)
				return;

			if (orientation == Orientation.Horizontal)
			{
				minimumSize = (int)MeasuredArrange.Value.Width;
			}
			else
			{
				minimumSize = (int)MeasuredArrange.Value.Height;
			}

		}

		public void Arrange(Rectangle rect)
		{

			if (rect.IsEmpty)
				return;

			if (rect != Allocation.ToRectangle())
			{
				MeasuredArrange = Measure(rect.Width, rect.Height, SizeRequestMode.ConstantSize);
				SizeAllocate(new Rectangle(rect.Location, RestrictToMeasuredArrange ? MeasuredArrange.Value : rect.Size).ToNative());
				QueueAllocate();
			}
		}

	}

}