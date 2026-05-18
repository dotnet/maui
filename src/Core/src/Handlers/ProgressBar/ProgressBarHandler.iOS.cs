using System;
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

			if (ContainerView is ProgressBarContainerView progressBarContainerView)
			{
				progressBarContainerView.LayoutProgressView();
			}
		}

		protected override void SetupContainer()
		{
			if (PlatformView == null || ContainerView != null)
			{
				return;
			}

			var oldParent = PlatformView.Superview;
			var oldIndex = oldParent?.IndexOfSubview(PlatformView);
			PlatformView.RemoveFromSuperview();

			ContainerView ??= new ProgressBarContainerView(PlatformView.Bounds);
			ContainerView.AddSubview(PlatformView);

			if (oldIndex is { } idx and >= 0)
			{
				oldParent?.InsertSubview(ContainerView, idx);
			}
			else
			{
				oldParent?.AddSubview(ContainerView);
			}
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