using System;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;
using Microsoft.Maui.Layouts;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;
using Size = Microsoft.Maui.Graphics.Size;

namespace Microsoft.Maui
{

	// refactored from: https://github.com/mono/xwt/blob/501f6b529fca632655295169094f637627c74c47/Xwt.Gtk/Xwt.GtkBackend/BoxBackend.cs

	public class LayoutView : Container, IGtkContainer
	{

		protected override bool OnDrawn(Cairo.Context cr)
		{
			var bk = this.GetBackgroundColor(this.StateFlags);

			if (bk != null)
			{
				cr.Save();
				cr.SetSourceColor(bk.ToCairoColor());
				cr.Rectangle(0, 0, Allocation.Width, Allocation.Height);

				cr.Fill();
				cr.Restore();
			}

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

		public bool IsReallocating;
		Dictionary<IView, Widget> _children = new();

		public LayoutView()
		{
			HasWindow = false;
		}

		public void ReplaceChild(Widget oldWidget, Widget newWidget)
		{
			var view = _children.FirstOrDefault(kvp => kvp.Value == oldWidget).Key;

			Remove(oldWidget);
			Add(newWidget);

			if (view != null)
			{
				_children[view] = newWidget;
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
				// .OrderBy(kvp => orientation == Orientation.Horizontal ? kvp.Value.Rect.X : kvp.Value.Rect.Y)
			   .Values
			   .ToArray();

			FocusChain = focusChain;
		}

		public void ClearChildren()
		{
			foreach (var c in Children)
			{
				Remove(c);
			}

			_children.Clear();
		}

		public void Add(IView view, Widget gw)
		{
			if (_children.ContainsKey(view))
				return;

			_children[view] = gw;

			Add(gw);
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

		protected void OnReallocate(Gdk.Rectangle allocation = default)
		{
			if (VirtualView == null)
			{
				return;
			}

			if (allocation == default)
			{
				allocation = new Gdk.Rectangle(0, 0, Allocation.Width, Allocation.Height);
			}

			var size = GetSizeRequest(allocation.Width, allocation.Height, SizeRequestMode.ConstantSize);

			if (size.Request.Width != allocation.Width || size.Request.Height != allocation.Height)
			{
				;
			}

		}

		protected void AllocateChildren(Gdk.Rectangle allocation)
		{
			var virtualView = VirtualView;

			if (virtualView == null)
			{
				return;
			}

			virtualView.InvalidateArrange();
			virtualView.Arrange(new Rectangle(allocation.X, allocation.Y, allocation.Width, allocation.Height));

			foreach (var cr in _children.ToArray())
			{
				var w = cr.Value;
				var v = cr.Key;
				var r = v.Frame;

				if (r.IsEmpty)
					continue;

				var cAlloc = new Gdk.Rectangle(allocation.X + (int)r.X, allocation.Y + (int)r.Y, (int)r.Width, (int)r.Height);

				// if (cAlloc != w.Allocation) // it's allways needed to implicit arrange children:
				w.SizeAllocate(cAlloc);
			}
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
			var virtualView = VirtualView;

			if (virtualView == null)
			{
				return;
			}

			try
			{
				IsReallocating = true;
				OnReallocate(allocation);
				AllocateChildren(allocation);
			}
			finally
			{
				IsReallocating = false;
			}

		}

		protected override void ForAll(bool includeInternals, Callback callback)
		{
			base.ForAll(includeInternals, callback);

			foreach (var c in _children.Values.ToArray())
				callback(c);
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
					OnReallocate();
				}
				catch
				{
					IsReallocating = false;
				}
			}

			base.OnRealized();
		}

		public SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint, SizeRequestMode mode)
		{
			var widthHandled = AllocatedWidth > 1; // && virtualView.DesiredSize.Width > 0;
			var heightHandled = AllocatedHeight > 1; // && virtualView.DesiredSize.Height > 0;
			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			var virtualView = VirtualView;

			if (virtualView == null)
			{
				return Size.Zero;
			}

			var withFactor = widthHandled && widthConstrained && widthConstraint > 1 ? widthConstraint / AllocatedWidth : 1;
			var heigthFactor = heightHandled && heightConstrained && heightConstraint > 1 ? heightConstraint / AllocatedHeight : 1;

			// if ((virtualView.Frame.Size.Width == widthConstraint ) && (virtualView.Frame.Size.Height == heightConstraint ))
			// {
			// 	return virtualView.Frame.Size;
			// }

			virtualView.InvalidateMeasure();
			var size1 = virtualView.Measure(widthConstraint, heightConstraint);

			return new SizeRequest(size1, size1);
		}

		int ToSize(double it) => double.IsPositiveInfinity(it) ? 0 : (int)it;

		int? lastHeight;
		int? lastWidth;

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimumSize, out int naturalSize)
		{
			base.OnAdjustSizeRequest(orientation, out minimumSize, out naturalSize);

			if (orientation == Orientation.Vertical && minimumSize != lastHeight)
			{
				;
			}

			if (orientation == Orientation.Horizontal && minimumSize != lastWidth)
			{
				;
			}
		}

		protected override void OnGetPreferredHeight(out int minimumHeight, out int naturalHeight)
		{
			SizeRequest size;

			if (RequestMode == SizeRequestMode.HeightForWidth)
			{
				OnGetPreferredWidth(out var minimumWidth, out var naturalWidth);
				lastWidth = lastWidth.HasValue ? Math.Min(minimumWidth, lastWidth.Value) : minimumWidth;
				size = GetSizeRequest(minimumWidth, double.PositiveInfinity, SizeRequestMode.HeightForWidth);
			}
			else
			{
				size = GetSizeRequest(double.PositiveInfinity, 0, SizeRequestMode.WidthForHeight);
			}

			minimumHeight = Math.Max(HeightRequest, ToSize(size.Minimum.Height));
			naturalHeight = Math.Max(HeightRequest, ToSize(size.Request.Height));
			lastHeight = lastHeight.HasValue ? Math.Min(minimumHeight, lastHeight.Value) : minimumHeight;
		}

		protected override void OnGetPreferredWidth(out int minimumWidth, out int naturalWidth)
		{
			SizeRequest size;

			if (RequestMode == SizeRequestMode.HeightForWidth)
			{
				size = GetSizeRequest(0, double.PositiveInfinity, SizeRequestMode.HeightForWidth);
			}
			else
			{
				GetPreferredHeight(out var minimumHeight, out var naturalHeight);
				lastHeight = minimumHeight;
				size = GetSizeRequest(0, minimumHeight, SizeRequestMode.WidthForHeight);
			}

			minimumWidth = Math.Max(WidthRequest, ToSize(size.Minimum.Width));
			naturalWidth = Math.Max(WidthRequest, ToSize(size.Request.Width));
			lastWidth = lastWidth.HasValue ? Math.Min(minimumWidth, lastWidth.Value) : minimumWidth;

		}

	}

}