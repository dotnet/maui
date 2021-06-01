using System;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
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

			var transform = await GetLayerTransformAsync(view);

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

			var transform = await GetLayerTransformAsync(view);

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

			var transform = await GetLayerTransformAsync(view);

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
			};

			var transform = await GetLayerTransformAsync(view);

			var expected = CATransform3D.MakeRotation(rotation * (float)Math.PI / 180.0f, 0, 0, 1);

			expected.AssertEqual(transform);
		}

		// TODO: this is all kinds of wrong
		protected Task<CATransform3D> GetLayerTransformAsync(TStub view)
		{
			var window = new WindowStub();

			window.View = view;
			view.Parent = window;

			view.Frame = new Rectangle(0, 0, 100, 100);

			return GetValueAsync(view, handler => GetLayerTransform(handler));
		}

		protected CATransform3D GetLayerTransform(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).Layer.Transform;

		protected string GetAutomationId(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityIdentifier;

		protected string GetSemanticDescription(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityLabel;

		protected string GetSemanticHint(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityHint;

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			((UIView)viewHandler.NativeView).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

		protected Visibility GetVisibility(IViewHandler viewHandler)
		{
			var nativeView = (UIView)viewHandler.NativeView;

			foreach (var constraint in nativeView.Constraints)
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

			if (nativeView.Hidden)
			{
				return Visibility.Hidden;
			}

			return Visibility.Visible;
		}
	}
}