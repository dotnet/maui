using System;
using System.Threading.Tasks;
using Android.Views;
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
			$"{((View)viewHandler.NativeView).GetTag(ViewExtensions.AutomationTagId)}";

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((View)viewHandler.NativeView).ContentDescription;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler)
		{
			// AccessibilityHeading is only available on API 28+
			// With lower Apis you use ViewCompat.SetAccessibilityHeading
			// but there exists no ViewCompat.GetAccessibilityHeading
			if (NativeVersion.IsAtLeast(28))
				return ((View)viewHandler.NativeView).AccessibilityHeading
					? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

			return viewHandler.VirtualView.Semantics.HeadingLevel;
		}

		protected float GetOpacity(IViewHandler viewHandler) =>
			((View)viewHandler.NativeView).Alpha;

		double GetTranslationX(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.Context.FromPixels(nativeView.TranslationX));
		}

		double GetTranslationY(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.Context.FromPixels(nativeView.TranslationY));
		}

		double GetScaleX(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.ScaleX);
		}

		double GetScaleY(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.ScaleY);
		}

		double GetRotation(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.Rotation);
		}

		double GetRotationX(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.RotationX);
		}

		double GetRotationY(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			return Math.Floor(nativeView.RotationY);
		}

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var nativeView = (View)viewHandler.NativeView;

			if (nativeView.Visibility == ViewStates.Visible)
				return Visibility.Visible;
			else if (nativeView.Visibility == ViewStates.Gone)
				return Visibility.Collapsed;
			else
				return Visibility.Hidden;
		}
	}
}