using System;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBasement<THandler, TStub>
	{
		// TODO: this is all kinds of wrong
		protected Task<CATransform3D> GetLayerTransformAsync(TStub view)
		{
			var window = new WindowStub();

			window.Content = view;
			view.Parent = window;

			view.Frame = new Rect(0, 0, 100, 100);

			return GetValueAsync(view, handler => GetLayerTransform(handler));
		}

		protected CATransform3D GetLayerTransform(IViewHandler viewHandler) =>
			((UIView)viewHandler.ToPlatform()).Layer.Transform;

		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.PlatformView).AccessibilityIdentifier;

		protected FlowDirection GetFlowDirection(IViewHandler viewHandler)
		{
			var platformView = (UIView)viewHandler.PlatformView;

			if (platformView.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft)
				return FlowDirection.RightToLeft;

			return FlowDirection.LeftToRight;
		}

		protected UIView GetAccessiblePlatformView(IViewHandler viewHandler)
		{
			var platformView = ((UIView)viewHandler.PlatformView);
			return platformView.GetAccessiblePlatformView();
		}

		protected Maui.Graphics.Rect GetPlatformViewBounds(IViewHandler viewHandler) =>
			viewHandler.VirtualView.ToPlatform().GetPlatformViewBounds();

		protected Maui.Graphics.Rect GetBoundingBox(IViewHandler viewHandler) =>
			viewHandler.VirtualView.ToPlatform().GetBoundingBox();

		protected System.Numerics.Matrix4x4 GetViewTransform(IViewHandler viewHandler) =>
			((UIView)viewHandler.ToPlatform()).GetViewTransform();

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			GetAccessiblePlatformView(viewHandler).AccessibilityLabel;

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			GetAccessiblePlatformView(viewHandler).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler)
		{
			var accessibilityTraits = GetAccessiblePlatformView(viewHandler).AccessibilityTraits;

			var hasHeader = (accessibilityTraits & UIAccessibilityTrait.Header) == UIAccessibilityTrait.Header;

			return hasHeader ? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;
		}

		protected nfloat GetOpacity(IViewHandler viewHandler) =>
			((UIView)viewHandler.ToPlatform()).Alpha;

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var platformView = (UIView)viewHandler.ToPlatform();

			foreach (var constraint in platformView.Constraints)
			{
				if (constraint is CollapseConstraint collapseConstraint)
				{
					// Active the collapse constraint; that will squish the view down to zero height
					if (collapseConstraint.Active)
					{
						return Visibility.Collapsed;
					}
				}
			}

			if (platformView.Hidden)
			{
				return Visibility.Hidden;
			}

			return Visibility.Visible;
		}

		protected bool GetUserInteractionEnabled(IViewHandler viewHandler)
		{
			var platformView = (UIView)viewHandler.PlatformView;
			return platformView.UserInteractionEnabled;
		}


#if !MACCATALYST
		protected void TapDoneOnInputAccessoryView(UIView platformView)
		{
			var accessoryView = (MauiDoneAccessoryView)platformView.InputAccessoryView;
			var doneButton = accessoryView.Items[1] as UIBarButtonItem;
			UIApplication.SharedApplication.SendAction(doneButton.Action, doneButton.Target!, null, null);
		}
#endif
	}
}