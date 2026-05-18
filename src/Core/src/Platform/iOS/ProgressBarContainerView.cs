using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal sealed class ProgressBarContainerView : WrapperView
	{
		public ProgressBarContainerView(CGRect frame)
			: base(frame)
		{
		}

		public override void LayoutSubviews()
		{
			ResetProgressViewTransform();
			base.LayoutSubviews();
			LayoutProgressView();
		}

		internal void LayoutProgressView()
		{
			if (Subviews.Length == 0 || Subviews[0] is not UIProgressView progressView)
			{
				return;
			}

			progressView.LayoutToBounds(Bounds);
		}

		void ResetProgressViewTransform()
		{
			if (Subviews.Length > 0 && Subviews[0] is UIProgressView progressView)
			{
				progressView.Transform = CGAffineTransform.MakeIdentity();
			}
		}
	}

	static class ProgressBarContainerViewExtensions
	{
		internal static void LayoutToBounds(this UIProgressView progressView, CGRect bounds)
		{
			progressView.Transform = CGAffineTransform.MakeIdentity();

			var targetWidth = bounds.Width < 0 ? 0 : bounds.Width;
			var targetHeight = bounds.Height < 0 ? 0 : bounds.Height;
			var naturalHeight = progressView.GetNaturalHeight(new CGSize(targetWidth, targetHeight));

			if (!IsPositiveFinite(naturalHeight) || !IsPositiveFinite(targetHeight))
			{
				progressView.Frame = new CGRect(bounds.X, bounds.Y, targetWidth, 0);
				return;
			}

			progressView.Bounds = new CGRect(0, 0, targetWidth, naturalHeight);
			progressView.Center = new CGPoint(bounds.GetMidX(), bounds.GetMidY());
			progressView.Transform = CGAffineTransform.MakeScale(1, targetHeight / naturalHeight);
		}

		static nfloat GetNaturalHeight(this UIProgressView progressView, CGSize size)
		{
			var naturalHeight = progressView.SizeThatFits(size).Height;

			if (!IsPositiveFinite(naturalHeight))
			{
				naturalHeight = progressView.IntrinsicContentSize.Height;
			}

			if (!IsPositiveFinite(naturalHeight))
			{
				naturalHeight = progressView.Bounds.Height;
			}

			return naturalHeight;
		}

		static bool IsPositiveFinite(nfloat value)
		{
			var doubleValue = (double)value;
			return doubleValue > 0 && !double.IsNaN(doubleValue) && !double.IsInfinity(doubleValue);
		}
	}
}
