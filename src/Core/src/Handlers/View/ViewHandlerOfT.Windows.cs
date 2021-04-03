using System;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public override void SetFrame(Rectangle rect)
		{
			var nativeView = NativeView;

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Measure(new Windows.Foundation.Size(rect.Size.Width, rect.Size.Height));
			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = NativeView;

			if (nativeView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var constraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			nativeView.Measure(constraint);
			var result = new Size(Math.Ceiling(nativeView.DesiredSize.Width), Math.Ceiling(nativeView.DesiredSize.Height));

			return new SizeRequest(result);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}