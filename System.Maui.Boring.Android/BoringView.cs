using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace System.Maui.Boring.Android
{
	public class BoringView : IFrameworkElement, IView
	{
		public Rectangle Frame { get; set; }

		public IViewRenderer Renderer { get; set; }

		public IFrameworkElement Parent { get; set; }

		public SizeRequest DesiredSize { get; set; }

		public bool IsMeasureValid { get; set; }

		public bool IsArrangeValid { get; set; }

		public bool IsEnabled { get; set; }

		public Xamarin.Forms.Color BackgroundColor { get; set; }

		public void Arrange(Rectangle bounds)
		{
			throw new NotImplementedException();
		}

		public void InvalidateArrange()
		{
			throw new NotImplementedException();
		}

		public void InvalidateMeasure()
		{
			throw new NotImplementedException();
		}

		public SizeRequest Measure(double widthConstraint, double heightConstraint)
		{
			throw new NotImplementedException();
		}
	}
}
