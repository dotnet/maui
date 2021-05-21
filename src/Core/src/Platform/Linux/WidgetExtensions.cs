using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class WidgetExtensions
	{

		public static void UpdateIsEnabled(this Widget nativeView, bool isEnabled) =>
			nativeView.Sensitive = isEnabled;

		public static void UpdateVisibility(this Widget nativeView, Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Hidden:
					nativeView.Visible = false;
					break;
				case Visibility.Visible:
					nativeView.Visible = true;
					break;
				case Visibility.Collapsed:
					
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
			}
		}

		public static SizeRequest GetDesiredSize(
			this Widget? nativeView,
			double widthConstraint,
			double heightConstraint)
		{
			if (nativeView == null)
				return Graphics.Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;
			
			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			if (nativeView.RequestMode == SizeRequestMode.HeightForWidth)
			{
				;
			}

			if (nativeView.RequestMode == SizeRequestMode.WidthForHeight)
			{
				;
			}

			if (nativeView.RequestMode == SizeRequestMode.ConstantSize)
			{
				;
			}

			if (!widthConstrained && !heightConstrained)
			{
				// https://developer.gnome.org/gtk3/stable/GtkWidget.html#gtk-widget-get-preferred-size
				nativeView.GetPreferredSize(out var minimumSize, out var req);

				return new SizeRequest(req.ToSize(), minimumSize.ToSize());
			}

			int minimumHeight = 0;
			int naturalHeight = 0;
			int minimumWidth = 0;
			int naturalWidth = 0;

			if (widthConstrained)
			{
				nativeView.GetPreferredHeightForWidth((int)widthConstraint, out minimumHeight, out naturalHeight);

				if (!heightConstrained)
				{
					nativeView.GetPreferredWidthForHeight(Math.Max(minimumHeight, naturalHeight), out minimumWidth, out naturalWidth);

				}

			}

			if (heightConstrained)
			{
				nativeView.GetPreferredWidthForHeight((int)heightConstraint, out minimumWidth, out naturalWidth);

				if (!widthConstrained)
				{
					nativeView.GetPreferredHeightForWidth(Math.Max(minimumWidth, naturalWidth), out minimumHeight, out naturalHeight);
				}

			}

			return new SizeRequest(new Size(naturalWidth, naturalHeight), new Size(minimumWidth, minimumHeight));
		}

		public static void Arrange(this Widget? nativeView, Rectangle rect)
		{
			if (nativeView == null)
				return;

			if (rect.IsEmpty)
				return;

			if (rect != nativeView.Allocation.ToRectangle())
			{
				nativeView.SizeAllocate(rect.ToNative());
				nativeView.QueueResize();
			}
		}

		public static void InvalidateMeasure(this Widget nativeView, IView view)
		{
			nativeView.QueueAllocate();
		}

		static int Request(double viewSize) => viewSize >= 0 ? (int)viewSize : -1;

		public static void UpdateWidth(this Widget nativeView, IView view)
		{
			var widthRequest = Request(view.Width);

			if (widthRequest != -1 && widthRequest != nativeView.WidthRequest && widthRequest != nativeView.AllocatedWidth)
			{
				nativeView.WidthRequest = widthRequest;
			}

		}

		public static void UpdateHeight(this Widget nativeView, IView view)
		{
			var heightRequest = Request(view.Height);

			if (heightRequest != -1 && heightRequest != nativeView.HeightRequest && heightRequest != nativeView.AllocatedHeight)
			{
				nativeView.HeightRequest = heightRequest;
			}

		}

		public static void UpdateFont(this Widget nativeView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;

			var fontFamily = fontManager.GetFontFamily(font);
#pragma warning disable 612
			nativeView.ModifyFont(fontFamily);
#pragma warning restore 612

		}

		public static void ReplaceChild(this Gtk.Container cont, Gtk.Widget oldWidget, Gtk.Widget newWidget)
		{
			if (oldWidget.Parent != cont)
				return;

			switch (cont)
			{
				case IGtkContainer container:
					container.ReplaceChild(oldWidget, newWidget);

					break;
				case Gtk.Notebook notebook:
				{
					Gtk.Notebook.NotebookChild nc = (Gtk.Notebook.NotebookChild)notebook[oldWidget];
					var detachable = nc.Detachable;
					var pos = nc.Position;
					var reorderable = nc.Reorderable;
					var tabExpand = nc.TabExpand;
					var tabFill = nc.TabFill;
					var label = notebook.GetTabLabel(oldWidget);
					notebook.Remove(oldWidget);
					notebook.InsertPage(newWidget, label, pos);

					nc = (Gtk.Notebook.NotebookChild)notebook[newWidget];
					nc.Detachable = detachable;
					nc.Reorderable = reorderable;
					nc.TabExpand = tabExpand;
					nc.TabFill = tabFill;

					break;
				}
				case Gtk.Paned paned:
				{
					var pc = (Gtk.Paned.PanedChild)paned[oldWidget];
					var resize = pc.Resize;
					var shrink = pc.Shrink;
					var pos = paned.Position;

					if (paned.Child1 == oldWidget)
					{
						paned.Remove(oldWidget);
						paned.Pack1(newWidget, resize, shrink);
					}
					else
					{
						paned.Remove(oldWidget);
						paned.Pack2(newWidget, resize, shrink);
					}

					paned.Position = pos;

					break;
				}
				case Gtk.Bin bin:
					bin.Remove(oldWidget);
					bin.Child = newWidget;

					break;
			}
		}

	}

}