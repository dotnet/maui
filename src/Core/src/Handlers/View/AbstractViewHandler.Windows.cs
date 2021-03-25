using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class AbstractViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public void SetFrame(Rectangle rect)
		{
			var nativeView = View;

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			if ((rect.Height >= 0 || rect.Width >= 0))
				nativeView.Measure(new Windows.Foundation.Size(rect.Size.Width, rect.Size.Height));

			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = View;

			if (nativeView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			nativeView.Measure(constraint);
			var result = new Size(Math.Ceiling(nativeView.DesiredSize.Width), Math.Ceiling(nativeView.DesiredSize.Height));

			return new SizeRequest(result);
		}

		void SetupContainer()
		{

		}

		void RemoveContainer()
		{

		}
	}
}