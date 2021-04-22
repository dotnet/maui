#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		FrameworkElement? INativeViewHandler.NativeView => (FrameworkElement?)base.NativeView;

		public override void SetFrame(Rectangle rect)
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

			var explicitWidth = VirtualView.Width;
			var explicitHeight = VirtualView.Height;
			var useExplicitWidth = explicitWidth >= 0;
			var useExplicitHeight = explicitHeight >= 0;

			if (useExplicitWidth)
			{
				widthConstraint = Math.Min(VirtualView.Width, widthConstraint);
			}

			if (useExplicitHeight)
			{
				heightConstraint = Math.Min(VirtualView.Height, heightConstraint);
			}

			var measureConstraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			NativeView.Measure(measureConstraint);

			var desiredWidth = NativeView.DesiredSize.Width;
			var desiredHeight = NativeView.DesiredSize.Height;

			var resultWidth = useExplicitWidth 
				? Math.Max(desiredWidth, explicitWidth) 
				: desiredWidth;

			var resultHeight = useExplicitHeight
				? Math.Max(desiredHeight, explicitHeight)
				: desiredHeight;

			return new Size(resultWidth, resultHeight);
		}

		protected override void SetupContainer()
		{

		}

		protected override void RemoveContainer()
		{

		}
	}
}