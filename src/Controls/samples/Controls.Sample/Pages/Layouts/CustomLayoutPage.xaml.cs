using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

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

	public class DockLayout : Layout
	{
		protected override ILayoutManager CreateLayoutManager()
		{
			return new DockLayoutManager(this);
		}

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


		class DockLayoutManager : LayoutManager
		{
			readonly DockLayout _layout;

			public DockLayoutManager(DockLayout layout) : base(layout)
			{
				_layout = layout;
			}

			public override Size ArrangeChildren(Rect bounds)
			{
				var (x, y, width, height) = bounds;
				Size sizeRequest = Size.Zero;
				int i = 0;

				foreach (View child in _layout)
				{
					if (child.IsVisible)
					{
						i++;

						double childX = 0;
						double childY = 0;
						Size request = sizeRequest;
						double childWidth = Math.Min(width, request.Width);
						double childHeight = Math.Min(height, request.Height);

						bool lastItem = i == _layout.Count;
						if (lastItem & _layout.LastChildFill)
						{
							((IView)child).Arrange(new Rect(x, y, width, height));
							return sizeRequest;
						}

						switch (_layout.GetDock(child))
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

						((IView)child).Arrange(new Rect(childX, childY, childWidth, childHeight));
					}
				}

				return sizeRequest;
			}

			public override Size Measure(double widthConstraint, double heightConstraint)
			{
				double height = 0;
				double width = 0;
				double finalWidth = 0;
				double finalHeight = 0;

				foreach (View child in _layout)
				{
					if (child.IsVisible)
					{
						var request = child.Measure(widthConstraint, heightConstraint);

						switch (_layout.GetDock(child))
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

				return new Size(finalWidth, finalHeight);
			}
		}
	}
}