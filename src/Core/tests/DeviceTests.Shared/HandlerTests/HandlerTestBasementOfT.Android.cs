using System;
using System.Numerics;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBasement<THandler, TStub>
	{
		protected string GetAutomationId(IViewHandler viewHandler) =>
			$"{GetSemanticPlatformElement(viewHandler).ContentDescription}";

		protected FlowDirection GetFlowDirection(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			if (platformView.LayoutDirection == LayoutDirection.Rtl)
				return FlowDirection.RightToLeft;

			return FlowDirection.LeftToRight;
		}

		public View GetSemanticPlatformElement(IViewHandler viewHandler)
		{
			return ((View)viewHandler.PlatformView).GetSemanticPlatformElement();
		}

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ContentDescription;

		protected string GetSemanticHint(IViewHandler viewHandler)
		{
			if (GetSemanticPlatformElement(viewHandler) is EditText et)
				return et.Hint;

			return null;
		}

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler)
		{
			// AccessibilityHeading is only available on API 28+
			// With lower Apis you use ViewCompat.SetAccessibilityHeading
			// but there exists no ViewCompat.GetAccessibilityHeading
			if (OperatingSystem.IsAndroidVersionAtLeast(28))
				return ((View)viewHandler.PlatformView).AccessibilityHeading
					? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

			return viewHandler.VirtualView.Semantics.HeadingLevel;
		}

		protected float GetOpacity(IViewHandler viewHandler) =>
			((View)viewHandler.PlatformView).Alpha;

		protected double GetTranslationX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Context.FromPixels(platformView.TranslationX));
		}

		protected double GetTranslationY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Context.FromPixels(platformView.TranslationY));
		}

		protected double GetScaleX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.ScaleX);
		}

		protected double GetScaleY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.ScaleY);
		}

		protected double GetRotation(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Rotation);
		}

		protected double GetRotationX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.RotationX);
		}

		protected double GetRotationY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.RotationY);
		}

		protected double GetMinHeight(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			var nativeHeight = platformView.MinimumHeight;

			var xplatHeight = platformView.Context.FromPixels(nativeHeight);

			return xplatHeight;
		}

		protected double GetMinWidth(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			var nativeWidth = platformView.MinimumWidth;

			var xplatWidth = platformView.Context.FromPixels(nativeWidth);

			return xplatWidth;
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			if (platformView.Visibility == ViewStates.Visible)
				return Visibility.Visible;
			else if (platformView.Visibility == ViewStates.Gone)
				return Visibility.Collapsed;
			else
				return Visibility.Hidden;
		}

		protected Maui.Graphics.Rect GetPlatformViewBounds(IViewHandler viewHandler) =>
			viewHandler.VirtualView.ToPlatform().GetPlatformViewBounds();

		protected Maui.Graphics.Rect GetBoundingBox(IViewHandler viewHandler) =>
			viewHandler.VirtualView.ToPlatform().GetBoundingBox();

		protected Matrix4x4 GetViewTransform(IViewHandler viewHandler) =>
			((View)viewHandler.PlatformView).GetViewTransform();
	}
}