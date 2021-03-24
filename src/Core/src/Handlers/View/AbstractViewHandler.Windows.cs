using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class AbstractViewHandler<TVirtualView, TNativeView>
	{
		public void SetFrame(Rectangle rect)
		{
			var nativeView = View;

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = View;

			if (nativeView == null)
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