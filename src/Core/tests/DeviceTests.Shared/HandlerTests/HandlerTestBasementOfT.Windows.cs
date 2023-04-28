using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBasement<THandler, TStub>
	{
		protected Task<Maui.Graphics.Rect> GetPlatformViewBounds(IViewHandler viewHandler)
		{
			var fe = viewHandler.VirtualView.ToPlatform();
			return fe.AttachAndRun(() => fe.GetPlatformViewBounds(), MauiContext);
		}

		protected System.Numerics.Matrix4x4 GetViewTransform(IViewHandler viewHandler) =>
			((FrameworkElement)viewHandler.PlatformView).GetViewTransform();

		protected Task<Maui.Graphics.Rect> GetBoundingBox(IViewHandler viewHandler)
		{
			var fe = viewHandler.VirtualView.ToPlatform();
			return fe.AttachAndRun(() => fe.GetBoundingBox(), MauiContext);
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			AutomationProperties.GetAutomationId((FrameworkElement)viewHandler.PlatformView);

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			AutomationProperties.GetName((FrameworkElement)viewHandler.PlatformView);

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			AutomationProperties.GetHelpText((FrameworkElement)viewHandler.PlatformView);

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			(SemanticHeadingLevel)AutomationProperties.GetHeadingLevel((FrameworkElement)viewHandler.PlatformView);

		protected double GetOpacity(IViewHandler viewHandler)
		{
			return viewHandler.ToPlatform().Opacity;
		}

		protected double GetTranslationX(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.TranslateX;
			return 0.5;
		}

		protected double GetTranslationY(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.TranslateY;
			return 0.5;
		}

		protected double GetScaleX(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.RenderTransform is ScaleTransform scale)
				return scale.ScaleX;
			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.ScaleX;
			return 1;
		}

		protected double GetScaleY(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.RenderTransform is ScaleTransform scale)
				return scale.ScaleY;
			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.ScaleY;
			return 1;
		}

		protected double GetRotation(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.Rotation;
			return 0;
		}

		protected double GetRotationX(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.Projection is PlaneProjection projection)
				return -projection.RotationX;
			return 0;
		}

		protected double GetRotationY(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.Projection is PlaneProjection projection)
				return -projection.RotationY;
			return 0;
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var platformView = viewHandler.ToPlatform();

			if (platformView.Visibility == UI.Xaml.Visibility.Visible && platformView.Opacity == 0)
				return Visibility.Hidden;
			else if (platformView.Visibility == UI.Xaml.Visibility.Collapsed)
				return Visibility.Collapsed;
			else
				return Visibility.Visible;
		}

		protected FlowDirection GetFlowDirection(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.FlowDirection == UI.Xaml.FlowDirection.LeftToRight)
				return FlowDirection.LeftToRight;

			return FlowDirection.RightToLeft;
		}

		protected bool GetHitTestVisible(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;
			return platformView.IsHitTestVisible;
		}
	}
}
