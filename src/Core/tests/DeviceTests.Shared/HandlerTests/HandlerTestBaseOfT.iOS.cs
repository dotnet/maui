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
				M11 = -1.4984f,
				M12 = -3.7087f,
				M21 = 1.8544f,
				M22 = -0.7492f,
				M33 = 2f,
				M41 = 10f,
				M42 = 30f,
				M44 = 1f,
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

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task InputTransparencyInitializesCorrectly(bool inputTransparent)
		{
			if (typeof(TStub).IsAssignableTo(typeof(ILayout)))
			{
				// The platform type for Layouts (LayoutView) always has UserInteractionEnabled
				// to allow for its children to be interacted with
				return;
			}

			var view = new TStub()
			{
				InputTransparent = inputTransparent
			};

			var uie = await GetValueAsync(view, handler => GetUserInteractionEnabled(handler));

			// UserInteractionEnabled should be the opposite value of InputTransparent 
			Assert.NotEqual(inputTransparent, uie);
		}

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

			if (platformView is UISearchBar searchBar)
				platformView = searchBar.GetSearchTextField();

			return platformView;
		}

		protected bool GetIsAccessibilityElement(IViewHandler viewHandler)
		{
			var platformView = GetAccessiblePlatformView(viewHandler);

			// UIControl elements when instantiated have IsAccessibilityElement set to false.
			// Once they are added to the visual tree then iOS transitions IsAccessibilityElement
			// to true. In code we only set non UIControl elements ourselves to true.
			if (platformView is UIControl)
				return true;

			return platformView.IsAccessibilityElement;
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

		protected SemanticHeadingLevel GetSemanticHeading(IViewHandler viewHandler) =>
			GetAccessiblePlatformView(viewHandler).AccessibilityTraits.HasFlag(UIAccessibilityTrait.Header)
				? SemanticHeadingLevel.Level1 : SemanticHeadingLevel.None;

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