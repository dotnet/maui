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
	public partial class HandlerTestBase<THandler, TStub>
	{
		[Theory(DisplayName = "TranslationX Initialize Correctly")]
		[InlineData(10)]
		[InlineData(50)]
		[InlineData(100)]
		public async Task TranslationXInitializeCorrectly(double translationX)
		{
			var view = new TStub()
			{
				TranslationX = translationX
			};

			var tX = await GetValueAsync(view, handler => GetTranslationX(handler));
			Assert.Equal(view.TranslationX, tX);
		}

		[Theory(DisplayName = "TranslationY Initialize Correctly")]
		[InlineData(10)]
		[InlineData(50)]
		[InlineData(100)]
		public async Task TranslationYInitializeCorrectly(double translationY)
		{
			var view = new TStub()
			{
				TranslationY = translationY
			};

			var tY = await GetValueAsync(view, handler => GetTranslationY(handler));
			Assert.Equal(view.TranslationY, tY);
		}

		[Theory(DisplayName = "ScaleX Initialize Correctly")]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ScaleXInitializeCorrectly(double scaleX)
		{
			var view = new TStub()
			{
				ScaleX = scaleX
			};

			var sX = await GetValueAsync(view, handler => GetScaleX(handler));
			Assert.Equal(view.ScaleX, sX);
		}

		[Theory(DisplayName = "ScaleY Initialize Correctly")]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ScaleYInitializeCorrectly(double scaleY)
		{
			var view = new TStub()
			{
				ScaleY = scaleY
			};

			var sY = await GetValueAsync(view, handler => GetScaleY(handler));
			Assert.Equal(view.ScaleY, sY);
		}

		[Theory(DisplayName = "Rotation Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationInitializeCorrectly(double rotation)
		{
			var view = new TStub()
			{
				Rotation = rotation
			};

			var r = await GetValueAsync(view, handler => GetRotation(handler));
			Assert.Equal(view.Rotation % 360, r % 360);
		}

		[Theory(DisplayName = "RotationX Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationXInitializeCorrectly(double rotationX)
		{
			var view = new TStub()
			{
				RotationX = rotationX
			};

			var rX = await GetValueAsync(view, handler => GetRotationX(handler));
			Assert.Equal(view.RotationX % 360, rX % 360);
		}

		[Theory(DisplayName = "RotationY Initialize Correctly")]
		[InlineData(0)]
		[InlineData(90)]
		[InlineData(180)]
		[InlineData(270)]
		[InlineData(360)]
		public async Task RotationYInitializeCorrectly(double rotationY)
		{
			var view = new TStub()
			{
				RotationY = rotationY
			};

			var rY = await GetValueAsync(view, handler => GetRotationY(handler));
			Assert.Equal(view.RotationY % 360, rY % 360);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InputTransparencyInitializesCorrectly(bool inputTransparent)
		{
			var view = new TStub()
			{
				InputTransparent = inputTransparent
			};

			var uie = await GetValueAsync(view, handler => GetHitTestVisible(handler));

			// HitTestVisible should be the opposite value of InputTransparent 
			if (view is ILayout && inputTransparent)
			{
				// InputTransparent doesn't actually affect hit test visibility for LayoutPanel. 
				// The panel itself needs to always be hit test visible so it can relay input to non-transparent children.
				Assert.True(uie);
				return;
			}

			// HitTestVisible should be the opposite value of InputTransparent 
			Assert.NotEqual(inputTransparent, uie);
		}

		protected Task<Maui.Graphics.Rect> GetPlatformViewBounds(IViewHandler viewHandler)
		{
			var fe = viewHandler.VirtualView.ToPlatform();
			return fe.AttachAndRun(() => fe.GetPlatformViewBounds());
		}

		protected System.Numerics.Matrix4x4 GetViewTransform(IViewHandler viewHandler) =>
			((FrameworkElement)viewHandler.PlatformView).GetViewTransform();

		protected Task<Maui.Graphics.Rect> GetBoundingBox(IViewHandler viewHandler)
		{
			var fe = viewHandler.VirtualView.ToPlatform();
			return fe.AttachAndRun(() => fe.GetBoundingBox());
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			AutomationProperties.GetAutomationId((FrameworkElement)viewHandler.PlatformView);

		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			((AccessibilityView)((FrameworkElement)viewHandler.PlatformView).GetValue(AutomationProperties.AccessibilityViewProperty)) == AccessibilityView.Content;

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			AutomationProperties.GetName((FrameworkElement)viewHandler.PlatformView);

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			AutomationProperties.GetHelpText((FrameworkElement)viewHandler.PlatformView);

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			(SemanticHeadingLevel)AutomationProperties.GetHeadingLevel((FrameworkElement)viewHandler.PlatformView);

		protected double GetOpacity(IViewHandler viewHandler) =>
			((FrameworkElement)viewHandler.PlatformView).Opacity;

		double GetTranslationX(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.TranslateX;
			return 0.5;
		}

		double GetTranslationY(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.TranslateY;
			return 0.5;
		}

		double GetScaleX(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.RenderTransform is ScaleTransform scale)
				return scale.ScaleX;
			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.ScaleX;
			return 1;
		}

		double GetScaleY(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.RenderTransform is ScaleTransform scale)
				return scale.ScaleY;
			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.ScaleY;
			return 1;
		}

		double GetRotation(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.RenderTransform is CompositeTransform composite)
				return composite.Rotation;
			return 0;
		}

		double GetRotationX(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.Projection is PlaneProjection projection)
				return -projection.RotationX;
			return 0;
		}

		double GetRotationY(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

			if (platformView.Projection is PlaneProjection projection)
				return -projection.RotationY;
			return 0;
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var platformView = (FrameworkElement)viewHandler.PlatformView;

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
