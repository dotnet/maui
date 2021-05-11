#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		FrameworkElement? INativeViewHandler.NativeView => (FrameworkElement?)base.NativeView;

		public override void NativeArrange(Rectangle rect)
		{
			var nativeView = NativeView;

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Arrange(new Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (NativeView == null || VirtualView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var measureConstraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			NativeView.Measure(measureConstraint);

			return new Size(NativeView.DesiredSize.Width, NativeView.DesiredSize.Height);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}