using Xamarin.Forms;
using Xamarin.Platform.Layouts;

namespace Xamarin.Platform
{
	public abstract class FrameworkElement : IFrameworkElement
	{
		public bool IsEnabled => true;

		public Color BackgroundColor { get; set; } = Color.Transparent;

		public Rectangle Frame 
		{ 
			get; 
			protected set; 
		}

		public IViewHandler Handler { get; set; }

		public IFrameworkElement Parent { get; set; }

		public Size DesiredSize { get; protected set; }

		public virtual bool IsMeasureValid { get; protected set; }

		public bool IsArrangeValid { get; protected set; }

		public double Width { get; set; } = -1;
		public double Height { get; set; } = -1;

		public virtual void Arrange(Rectangle bounds)
		{
			Frame = bounds;
		}

		public void InvalidateMeasure()
		{
			IsMeasureValid = false;
			IsArrangeValid = false;
		}

		public void InvalidateArrange()
		{
			IsArrangeValid = false;
		}

		public virtual Size Measure(double widthConstraint, double heightConstraint)
		{
			if (!IsMeasureValid)
			{
				if (Handler == null)
				{
					DesiredSize = Size.Zero;
				}
				else
				{
					widthConstraint = LayoutManager.ResolveConstraints(widthConstraint, Width);
					heightConstraint = LayoutManager.ResolveConstraints(heightConstraint, Height);

					DesiredSize = Handler.GetDesiredSize(widthConstraint, heightConstraint);
				}
			}

			IsMeasureValid = true;
			return DesiredSize;
		}
	}
}
