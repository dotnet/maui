using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomLayoutPage
	{
		public CustomLayoutPage()
		{
			InitializeComponent();
		}
	}

	public enum Dock
	{
		Left,
		Top,
		Right,
		Bottom
	}

	public class DockLayout : Layout<View>
	{
		public static readonly BindableProperty DockProperty =
			BindableProperty.Create(nameof(Dock), typeof(Dock), typeof(DockLayout), Dock.Left,
				BindingMode.TwoWay, null);

		public Dock Dock
		{
			get { return (Dock)GetValue(DockProperty); }
			set { SetValue(DockProperty, value); }
		}

		private Dock GetDock(BindableObject bindable)
		{
			return (Dock)bindable.GetValue(DockProperty);
		}

		public static readonly BindableProperty LastChildFillProperty =
		   BindableProperty.Create(nameof(LastChildFill), typeof(bool), typeof(DockLayout), true,
			   BindingMode.TwoWay, null);

		/// <summary>
		/// The default behavior is that the last child of the DockLayout takes up the rest of the space, 
		/// but this can be disabled using the LastChildFill.
		/// </summary>
		public bool LastChildFill
		{
			get { return (bool)GetValue(LastChildFillProperty); }
			set { SetValue(LastChildFillProperty, value); }
		}

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
			SizeRequest sizeRequest = new SizeRequest();
			int i = 0;

			foreach (var child in Children)
			{
				if (child.IsVisible)
				{
					i++;

					sizeRequest = child.Measure(width, height, MeasureFlags.IncludeMargins);

					double childX = 0;
					double childY = 0;
					Size request = sizeRequest.Request;
					double childWidth = Math.Min(width, request.Width);
					double childHeight = Math.Min(height, request.Height);

					bool lastItem = i == Children.Count;
					if (lastItem & LastChildFill)
					{
						LayoutChildIntoBoundingRegion(child, new Rect(x, y, width, height));
						return;
					}

					switch (GetDock(child))
					{
						case Dock.Left:
							{
								childX = x;
								childY = y;
								childHeight = height;
								x += childWidth;
								width -= childWidth;
								break;
							}
						case Dock.Top:
							{
								childX = x;
								childY = y;
								childWidth = width;
								y += childHeight;
								height -= childHeight;
								break;
							}
						case Dock.Right:
							{
								childX = x + width - childWidth;
								childY = y;
								childHeight = height;
								width -= childWidth;
								break;
							}
						case Dock.Bottom:
							{
								childX = x;
								childY = y + height - childHeight;
								childWidth = width;
								height -= childHeight;
								break;
							}
						default:
							{
								goto case Dock.Left;
							}
					}

					LayoutChildIntoBoundingRegion(child, new Rect(childX, childY, childWidth, childHeight));
				}
			}
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			double height = 0;
			double width = 0;
			double finalWidth = 0;
			double finalHeight = 0;

			foreach (var child in Children)
			{
				if (child.IsVisible)
				{
					SizeRequest sizeRequest = child.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
					Size request = sizeRequest.Request;

					switch (GetDock(child))
					{
						case Dock.Left:
						case Dock.Right:
							{
								width += request.Width;
								finalWidth = Math.Max(finalWidth, width);
								finalHeight = Math.Max(finalHeight, height + request.Height);
								break;
							}
						case Dock.Top:
						case Dock.Bottom:
							{
								height += request.Height;
								finalWidth = Math.Max(finalWidth, width + request.Width);
								finalHeight = Math.Max(finalHeight, height);
								break;
							}
						default:
							{
								goto case Dock.Right;
							}
					}
				}
			}
			return new SizeRequest(new Size(finalWidth, finalHeight));
		}
	}
}