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

		[Theory]
		[InlineData(0)]
		[InlineData(100)]
		public async Task MinimumHeightInitializes(double minHeight)
		{
			var view = new TStub()
			{
				MinimumHeight = minHeight
			};

			var expected = view.MinimumHeight;
			var result = await GetValueAsync(view, handler => GetMinHeight(handler));

			Assert.Equal(expected, result, 0);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(100)]
		public async Task MinimumWidthInitializes(double minWidth)
		{
			var view = new TStub()
			{
				MinimumWidth = minWidth
			};

			var expected = view.MinimumWidth;
			var result = await GetValueAsync(view, handler => GetMinWidth(handler));

			Assert.Equal(expected, result, 0);
		}

		[Fact]
		public async Task NeedsContainerWhenInputTransparent()
		{
			var view = new TStub()
			{
				InputTransparent = true
			};

			var handler = await CreateHandlerAsync(view);

			if (handler is ViewHandler vh)
				Assert.True(vh.NeedsContainer);
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			$"{GetSemanticPlatformElement(viewHandler).ContentDescription}";

		protected FlowDirection GetFlowDirection(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			if (platformView.LayoutDirection == LayoutDirection.Rtl)
				return FlowDirection.RightToLeft;

			return FlowDirection.LeftToRight;
		}

		protected bool GetIsAccessibilityElement(IViewHandler viewHandler) =>
			GetSemanticPlatformElement(viewHandler).ImportantForAccessibility == ImportantForAccessibility.Yes;

		public View GetSemanticPlatformElement(IViewHandler viewHandler)
		{
			if (viewHandler.PlatformView is View sv)
				return sv.GetSemanticPlatformElement();

			return (View)viewHandler.PlatformView;
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

		double GetTranslationX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Context.FromPixels(platformView.TranslationX));
		}

		double GetTranslationY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Context.FromPixels(platformView.TranslationY));
		}

		double GetScaleX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.ScaleX);
		}

		double GetScaleY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.ScaleY);
		}

		double GetRotation(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.Rotation);
		}

		double GetRotationX(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.RotationX);
		}

		double GetRotationY(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			return Math.Floor(platformView.RotationY);
		}

		double GetMinHeight(IViewHandler viewHandler)
		{
			var platformView = (View)viewHandler.PlatformView;

			var nativeHeight = platformView.MinimumHeight;

			var xplatHeight = platformView.Context.FromPixels(nativeHeight);

			return xplatHeight;
		}

		double GetMinWidth(IViewHandler viewHandler)
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