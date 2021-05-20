#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class FrameworkElementHandler<TVirtualView, TNativeView> : INativeViewHandler
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

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}