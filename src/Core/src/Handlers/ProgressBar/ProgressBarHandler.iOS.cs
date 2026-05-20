using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ProgressBarHandler : ViewHandler<IProgress, UIProgressView>
	{
		public override bool NeedsContainer => true;

		protected override UIProgressView CreatePlatformView()
		{
			return new UIProgressView(UIProgressViewStyle.Default);
		}

		public override void PlatformArrange(Rect rect)
		{
			base.PlatformArrange(rect);

			LayoutProgressView();
		}

		public static void MapProgress(IProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgress(progress);
		}

		public static void MapProgressColor(IProgressBarHandler handler, IProgress progress)
		{
			handler.PlatformView?.UpdateProgressColor(progress);
		}

		internal static void MapFlowDirection(IProgressBarHandler handler, IProgress progress)
		{
			var progressbar = handler.PlatformView;
			if (progressbar is null)
			{
				return;
			}

			UISemanticContentAttribute contentAttribute = GetSemanticContentAttribute(progress);
			progressbar.SemanticContentAttribute = contentAttribute;

			// On iOS 26, UIProgressView no longer applies the SemanticContentAttribute to its internal subviews, so update
			// each subview explicitly to keep flow direction consistent.
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
			{
				foreach (var subview in progressbar.Subviews)
				{
					subview.SemanticContentAttribute = contentAttribute;
				}
			}
		}

		void LayoutProgressView()
		{
			if (PlatformView is null || ContainerView is null)
			{
				return;
			}

			LayoutProgressView(PlatformView, ContainerView.Bounds);
		}

		static void LayoutProgressView(UIProgressView progressView, CGRect bounds)
		{
			progressView.Transform = CGAffineTransform.MakeIdentity();

			var targetWidth = bounds.Width < 0 ? 0 : bounds.Width;
			var targetHeight = bounds.Height < 0 ? 0 : bounds.Height;
			var naturalHeight = GetNaturalHeight(progressView, new CGSize(targetWidth, targetHeight));

			if (!IsPositiveFinite(naturalHeight) || !IsPositiveFinite(targetHeight))
			{
				progressView.Frame = new CGRect(bounds.X, bounds.Y, targetWidth, 0);
				return;
			}

			progressView.Bounds = new CGRect(0, 0, targetWidth, naturalHeight);
			progressView.Center = new CGPoint(bounds.GetMidX(), bounds.GetMidY());
			progressView.Transform = CGAffineTransform.MakeScale(1, targetHeight / naturalHeight);
		}

		static nfloat GetNaturalHeight(UIProgressView progressView, CGSize size)
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

		static UISemanticContentAttribute GetSemanticContentAttribute(IProgress progress)
		{
			return progress.FlowDirection switch
			{
				FlowDirection.RightToLeft => UISemanticContentAttribute.ForceRightToLeft,
				FlowDirection.LeftToRight => UISemanticContentAttribute.ForceLeftToRight,
				_ => GetParentSemanticContentAttribute(progress)
			};
		}

		static UISemanticContentAttribute GetParentSemanticContentAttribute(IProgress progress)
		{
			var parentView = (progress as IView)?.Parent as IView;
			if (parentView is null)
			{
				return UISemanticContentAttribute.Unspecified;
			}

			return parentView.FlowDirection switch
			{
				FlowDirection.LeftToRight => UISemanticContentAttribute.ForceLeftToRight,
				FlowDirection.RightToLeft => UISemanticContentAttribute.ForceRightToLeft,
				_ => UISemanticContentAttribute.Unspecified,
			};
		}
	}
}
