using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui
{

	public static class WidgetExtensions
	{

		public static void UpdateIsEnabled(this Widget platformView, bool isEnabled) =>
			platformView.Sensitive = isEnabled;

		public static void UpdateVisibility(this Widget platformView, Visibility visibility)
		{
			switch (visibility)
			{
				case Visibility.Hidden:
					platformView.Visible = false;

					break;
				case Visibility.Visible:
					platformView.Visible = true;

					break;
				case Visibility.Collapsed:

					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
			}
		}

		public static SizeRequest GetDesiredSize(
			this Widget? platformView,
			double widthConstraint,
			double heightConstraint)
		{
			if (platformView == null)
				return Graphics.Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var widthConstrained = !double.IsPositiveInfinity(widthConstraint);
			var heightConstrained = !double.IsPositiveInfinity(heightConstraint);

			int minimumHeight = 0;
			int naturalHeight = 0;
			int minimumWidth = 0;
			int naturalWidth = 0;
#pragma warning disable CS0162 // Unreachable code detected

			if (false &&(!widthConstrained && !heightConstrained))
			{
				platformView.GetSizeRequest(out var w, out var h);

				return new SizeRequest(new Size(w, h));
			}

			if (platformView.RequestMode == SizeRequestMode.WidthForHeight)
			{
				if ((heightConstrained) && (widthConstrained))
				{
					minimumWidth = naturalWidth = (int)widthConstraint;
					minimumHeight = naturalHeight = (int)heightConstraint;
				}
				else if (widthConstrained)
				{
					minimumWidth = naturalWidth = (int)widthConstraint;
					platformView.GetPreferredHeightForWidth((int)widthConstraint, out minimumHeight, out naturalHeight);
				}
				else if (heightConstrained)
				{
					minimumHeight = naturalHeight = (int)heightConstraint;
					platformView.GetPreferredWidthForHeight((int)heightConstraint, out minimumWidth, out naturalWidth);
				}

				else
				{
					platformView.GetPreferredHeight(out minimumHeight, out naturalHeight);
					platformView.GetPreferredWidthForHeight(minimumHeight, out minimumWidth, out naturalWidth);
				}
			}
			else if (platformView.RequestMode == Gtk.SizeRequestMode.HeightForWidth)
			{
				if ((heightConstrained) && (widthConstrained))
				{
					minimumWidth = naturalWidth = (int)widthConstraint;
					minimumHeight = naturalHeight = (int)heightConstraint;
				}
				else if (heightConstrained)
				{
					minimumHeight = naturalHeight = (int)heightConstraint;
					platformView.GetPreferredWidthForHeight((int)heightConstraint, out minimumWidth, out naturalWidth);
				}
				else if (widthConstrained)
				{
					minimumWidth = naturalWidth = (int)widthConstraint;
					platformView.GetPreferredHeightForWidth((int)widthConstraint, out minimumHeight, out naturalHeight);
				}

				else
				{
					platformView.GetPreferredWidth(out minimumWidth, out naturalWidth);
					platformView.GetPreferredHeightForWidth(minimumWidth, out minimumHeight, out naturalHeight);
				}
			}
			else
			{
				platformView.GetPreferredWidth(out minimumWidth, out naturalWidth);
				platformView.GetPreferredHeightForWidth(minimumWidth, out minimumHeight, out naturalHeight);
			}

			if (platformView.WidthRequest > minimumWidth)
				minimumWidth = platformView.WidthRequest;

			if (platformView.HeightRequest > minimumHeight)
				minimumHeight = platformView.HeightRequest;

			return new SizeRequest(new Size(naturalWidth, naturalHeight), new Size(minimumWidth, minimumHeight));
#pragma warning restore CS0162 // Unreachable code detected
		}

		public static void Arrange(this Widget? platformView, Rect rect)
		{
			if (platformView == null)
				return;

			if (rect.IsEmpty)
				return;

			if (rect != platformView.Allocation.ToRect())
			{
				platformView.SizeAllocate(rect.ToNative());
				platformView.QueueAllocate();
			}
		}

		public static void InvalidateMeasure(this Widget platformView, IView view)
		{
			platformView.QueueAllocate();
		}

		public static int Request(double viewSize) => viewSize >= 0 ? (int)viewSize : -1;

		public static void UpdateWidth(this Widget platformView, IView view)
		{
			var widthRequest = Request(view.Width);

			if (widthRequest != -1 && widthRequest != platformView.WidthRequest && widthRequest != platformView.AllocatedWidth)
			{
				platformView.WidthRequest = widthRequest;
				platformView.QueueResize();
			}
		}

		public static void UpdateHeight(this Widget platformView, IView view)
		{
			var heightRequest = Request(view.Height);

			if (heightRequest != -1 && heightRequest != platformView.HeightRequest && heightRequest != platformView.AllocatedHeight)
			{
				platformView.HeightRequest = heightRequest;
				platformView.QueueResize();
			}
		}

		public static void UpdateFont(this Widget platformView, ITextStyle textStyle, IFontManager fontManager)
		{
			var font = textStyle.Font;

			var fontFamily = fontManager.GetFontFamily(font);
#pragma warning disable 612
			platformView.ModifyFont(fontFamily);
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

		[MissingMapper]
		public static void UpdateMinimumHeight(this Widget platformView, IView view)
		{
			UpdateHeight(platformView, view);
		}

		[MissingMapper]
		public static void UpdateMinimumWidth(this Widget platformView, IView view)
		{
			UpdateWidth(platformView, view);
		}

		[MissingMapper]
		public static void UpdateMaximumHeight(this Widget platformView, IView view) { }

		[MissingMapper]
		public static void UpdateMaximumWidth(this Widget platformView, IView view) { }

	}

}