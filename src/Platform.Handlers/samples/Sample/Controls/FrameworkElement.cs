using Xamarin.Forms;
using Xamarin.Platform;
using Xamarin.Platform.Layouts;

namespace Sample
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

		public Thickness Margin { get; set; } = Thickness.Zero;

		public virtual void Arrange(Rectangle bounds)
		{
			if (IsArrangeValid)
			{
				return;
			}

			Frame = this.ComputeFrame(bounds);
			IsArrangeValid = true;

			Handler?.SetFrame(Frame);
		}

		public virtual void InvalidateMeasure()
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
				DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			}

			IsMeasureValid = true;
			return DesiredSize;
		}
	}
}
