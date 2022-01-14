using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static partial class ViewHandlerExtensions
	{
		internal static Size GetDesiredSizeFromHandler(this IViewHandler viewHandler, double widthConstraint, double heightConstraint)
		{
			var nativeView = viewHandler.GetWrappedNativeView();

			if (nativeView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var measureConstraint = new global::Windows.Foundation.Size(widthConstraint, heightConstraint);

			nativeView.Measure(measureConstraint);

			return new Size(nativeView.DesiredSize.Width, nativeView.DesiredSize.Height);
		}

		internal static void NativeArrangeHandler(this IViewHandler viewHandler, Rectangle rect)
		{
			var nativeView = viewHandler.GetWrappedNativeView();

			if (nativeView == null)
				return;

			if (rect.Width < 0 || rect.Height < 0)
				return;

			nativeView.Arrange(new global::Windows.Foundation.Rect(rect.X, rect.Y, rect.Width, rect.Height));

			viewHandler.Invoke(nameof(IView.Frame), rect);
		}
	}
}
