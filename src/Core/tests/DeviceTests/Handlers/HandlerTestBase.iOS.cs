using System;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class HandlerTestBase<THandler, TStub>
	{
		[Fact(DisplayName = "Transformation Initialize Correctly")]
		public async Task TransformationInitializeCorrectly()
		{
			var view = new TStub()
			{
				TranslationX = 10,
				TranslationY = 10,
				Scale = 1.2,
				Rotation = 90
			};

			var transform = await GetValueAsync(view, handler => GetLayerTransform(handler));

			Assert.NotEqual(CATransform3D.Identity, transform);
		}

		[Fact(DisplayName = "Transformation Calculated Correctly")]
		public async Task TransformationCalculatedCorrectly()
		{
			var view = new TStub()
			{
				TranslationX = 10.0,
				TranslationY = 30.0,
				Rotation = 248.0,
				Scale = 2.0,
				ScaleX = 2.0,
			};

			var transform = await GetValueAsync(view, handler => GetLayerTransform(handler));

			var expected = new CATransform3D
			{
				m11 = -1.4984f,
				m12 = -3.7087f,
				m21 = 1.8544f,
				m22 = -0.7492f,
				m33 = 2f,
				m41 = 10f,
				m42 = 30f,
				m44 = 1f,
			};

			expected.AssertEqual(transform);
		}

		[Theory("Scale initializes correctly")]
		[InlineData(-10)]
		[InlineData(-3)]
		[InlineData(-1.5)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(1.5)]
		[InlineData(3)]
		[InlineData(10)]
		public async Task ScaleInitializeCorrectly(float scale)
		{
			var view = new TStub()
			{
				Scale = scale,
			};

			var transform = await GetValueAsync(view, handler => GetLayerTransform(handler));

			var expected = CATransform3D.MakeScale(scale, scale, scale);

			expected.AssertEqual(transform);
		}

		[Theory("Rotation initializes correctly")]
		[InlineData(-270)]
		[InlineData(-180)]
		[InlineData(-30)]
		[InlineData(-45)]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(45)]
		[InlineData(30)]
		[InlineData(180)]
		[InlineData(270)]
		public async Task RotationInitializeCorrectly(float rotation)
		{
			var view = new TStub()
			{
				Rotation = rotation,
				Width = 100,
				Height = 100
			};

			var transform = await GetValueAsync(view, handler => GetLayerTransform(handler));

			var expected = CATransform3D.MakeRotation(rotation, 0, 0, 1);

			expected.AssertEqual(transform);
		}

		protected THandler CreateHandler(IView view)
		{
			var handler = Activator.CreateInstance<THandler>();
			handler.SetMauiContext(MauiContext);

			handler.SetVirtualView(view);
			view.Handler = handler;

			return handler;
		}

		protected CATransform3D GetLayerTransform(IViewHandler viewHandler)
		{
			var view = (UIView)viewHandler.NativeView;

			var transform = view.Layer.Transform;

			return transform;
		}

		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityLabel;

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;
	}
}