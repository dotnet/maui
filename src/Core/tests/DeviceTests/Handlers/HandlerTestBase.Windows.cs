using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
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
			Assert.Equal(view.Rotation, r);
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
			Assert.Equal(view.RotationX, rX);
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
			Assert.Equal(view.RotationY, rY);
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			AutomationProperties.GetAutomationId((FrameworkElement)viewHandler.NativeView);

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			AutomationProperties.GetName((FrameworkElement)viewHandler.NativeView);

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			(SemanticHeadingLevel)AutomationProperties.GetHeadingLevel((FrameworkElement)viewHandler.NativeView);

		protected double GetOpacity(IViewHandler viewHandler) =>
			((FrameworkElement)viewHandler.NativeView).Opacity;

		double GetTranslationX(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			return nativeView.RenderTransformOrigin.X;
		}

		double GetTranslationY(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			return nativeView.RenderTransformOrigin.Y;
		}

		double GetScaleX(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.RenderTransform is ScaleTransform scale)
				return scale.ScaleX;
			if (nativeView.RenderTransform is CompositeTransform composite)
				return composite.ScaleY;
			return 0;
		}

		double GetScaleY(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.RenderTransform is ScaleTransform scale)
				return scale.ScaleY;
			if (nativeView.RenderTransform is CompositeTransform composite)
				return composite.ScaleY;
			return 0;
		}

		double GetRotation(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			return nativeView.Rotation;
		}

		double GetRotationX(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.Projection is PlaneProjection projection)
				return projection.RotationX;
			return 0;
		}

		double GetRotationY(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.Projection is PlaneProjection projection)
				return projection.RotationY;
			return 0;
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.Visibility == UI.Xaml.Visibility.Visible)
				return Visibility.Visible;
			else if (nativeView.Visibility == UI.Xaml.Visibility.Collapsed)
				return Visibility.Collapsed;
			else
				return Visibility.Hidden;
		}

		protected FlowDirection GetFlowDirection(IViewHandler viewHandler)
		{
			var nativeView = (FrameworkElement)viewHandler.NativeView;

			if (nativeView.FlowDirection == UI.Xaml.FlowDirection.LeftToRight)
				return FlowDirection.LeftToRight;

			return FlowDirection.RightToLeft;
		}
	}
}