using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewHandlerTests
	{
		[Fact]
		public void SafeAreaPixelComparisonUsesProvidedDisplayScale()
		{
			var subPixelInset = new SafeAreaPadding(0.24, 0, 0, 0);

			Assert.True(SafeAreaPadding.Empty.EqualsAtPixelLevel(subPixelInset, 2));
			Assert.False(SafeAreaPadding.Empty.EqualsAtPixelLevel(subPixelInset, 3));
		}

		[Theory]
		[InlineData(0.24, 2, true)]
		[InlineData(0.24, 3, false)]
		public void SafeAreaEmptyComparisonUsesProvidedDisplayScale(double value, double scale, bool expected)
		{
			Assert.Equal(expected, new SafeAreaPadding(value, 0, 0, 0).IsEmptyAtPixelLevel(scale));
		}

		[Theory]
		[InlineData(0.24, 2, false)]
		[InlineData(0.24, 3, true)]
		public void SafeAreaNonZeroComparisonUsesProvidedDisplayScale(double value, double scale, bool expected)
		{
			Assert.Equal(expected, SafeAreaPadding.IsNonZeroAtPixelLevel(value, scale));
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task EffectiveDisplayScaleUsesAttachedViewContentScale(double expected)
		{
			await InvokeOnMainThreadAsync(() =>
			{
#pragma warning disable CA1422 // A scene-less window is intentional to isolate the view's effective scale.
				using var window = new UIWindow();
#pragma warning restore CA1422
				using var view = new UIView();
				window.AddSubview(view);
				view.ContentScaleFactor = (nfloat)expected;

				Assert.Equal(expected, view.GetEffectiveDisplayScale());
			});
		}

		[Fact]
		public async Task DisplayScaleUsesOwningWindowScreen()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				UIWindowScene scene = null;
				foreach (var connectedScene in UIApplication.SharedApplication.ConnectedScenes)
				{
					if (connectedScene is UIWindowScene windowScene)
					{
						scene = windowScene;
						break;
					}
				}

#pragma warning disable CA1422 // Older test hosts have no scene and exercise the window fallback.
				using var window = scene is not null ? new UIWindow(scene) : new UIWindow();
#pragma warning restore CA1422
				using var view = new UIView();
				window.AddSubview(view);

				var expected = (double)(scene?.Screen.Scale ?? window.Screen.Scale);
				Assert.Same(window, view.Window);
				Assert.Equal(expected, view.GetDisplayScale());
			});
		}

		[Theory]
		[InlineData(3.0, 2.0, 1.0, 3.0)]
		[InlineData(1.0, 3.0, 2.0, 1.0)]
		[InlineData(null, 3.0, 1.0, 3.0)]
		[InlineData(null, null, 2.0, 2.0)]
		[InlineData(0.0, 2.0, 3.0, 1.0)]
		[InlineData(null, 0.0, 3.0, 1.0)]
		[InlineData(null, null, 0.0, 1.0)]
		public void DisplayScaleResolutionPrefersOwningDisplay(double? sceneScale, double? windowScale, double mainScale, double expected)
		{
			Assert.Equal(expected, Microsoft.Maui.Platform.ViewExtensions.ResolveDisplayScale(sceneScale, windowScale, mainScale));
		}

		[Theory]
		[InlineData(3.0, 2.0, 1.0, 3.0)]
		[InlineData(null, 3.0, 1.0, 3.0)]
		public async Task BitmapRenderingUsesIndependentlySpecifiedOwningDisplayScale(double? sceneScale, double? windowScale, double mainScale, double expected)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var scale = Microsoft.Maui.Platform.ViewExtensions.ResolveDisplayScale(sceneScale, windowScale, mainScale);
				using var format = new UIGraphicsImageRendererFormat { Opaque = false, Scale = 1 };
				using var renderer = new UIGraphicsImageRenderer(new CGSize(4, 4), format);
				using var sourceImage = renderer.CreateImage(context =>
				{
					UIColor.White.SetFill();
					context.FillRect(new CGRect(0, 0, 4, 4));
				});
				using var image = TextFieldExtensions.GetClearButtonTintImage(sourceImage, UIColor.Red, (nfloat)scale);

				Assert.NotNull(image);
				Assert.Equal(expected, image.CurrentScale);
				Assert.Equal((nint)(4 * expected), image.CGImage.Width);
				Assert.Equal((nint)(4 * expected), image.CGImage.Height);
			});
		}

		[Fact]
		public async Task BitmapRenderingUsesDisplayScaleInsteadOfEffectiveViewScale()
		{
			await InvokeOnMainThreadAsync(() =>
			{
#pragma warning disable CA1422 // A scene-less window is intentional to isolate the scale fallback behavior.
				using var window = new UIWindow();
#pragma warning restore CA1422
				using var view = new UIView();
				window.AddSubview(view);

				var displayScale = (double)window.Screen.Scale;
				var effectiveScale = displayScale == 1 ? 2 : 1;
				view.ContentScaleFactor = (nfloat)effectiveScale;

				Assert.Equal(displayScale, view.GetDisplayScale());
				Assert.Equal(effectiveScale, view.GetEffectiveDisplayScale());
				Assert.NotEqual(displayScale, effectiveScale);

				var sourceRenderer = new UIGraphicsImageRenderer(
					new CGSize(4, 4),
					new UIGraphicsImageRendererFormat { Opaque = false, Scale = 1 });
				using var sourceImage = sourceRenderer.CreateImage(context =>
				{
					UIColor.White.SetFill();
					context.FillRect(new CGRect(0, 0, 4, 4));
				});
				using var renderedImage = TextFieldExtensions.GetClearButtonTintImage(
					sourceImage,
					UIColor.Red,
					(nfloat)view.GetDisplayScale());

				Assert.NotNull(renderedImage);
				Assert.Equal(displayScale, renderedImage.CurrentScale);
				Assert.Equal((nint)(sourceImage.Size.Width * displayScale), renderedImage.CGImage.Width);
				Assert.Equal((nint)(sourceImage.Size.Height * displayScale), renderedImage.CGImage.Height);
			});
		}

		[Fact]
		public async Task KeyboardTransitionsInvalidateDescendantSafeAreaCache()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var parent = new Microsoft.Maui.Platform.ContentView();
				using var child = new Microsoft.Maui.Platform.ContentView();
				parent.AddSubview(child);

				var cacheValidField = typeof(MauiView).GetField(
					"_blockedEdgesCacheValid",
					BindingFlags.Instance | BindingFlags.NonPublic);
				var keyboardWillShowMethod = typeof(MauiView).GetMethod(
					"OnKeyboardWillShow",
					BindingFlags.Instance | BindingFlags.NonPublic);
				var clearKeyboardStateMethod = typeof(MauiView).GetMethod(
					"ClearKeyboardState",
					BindingFlags.Instance | BindingFlags.NonPublic);

				Assert.NotNull(cacheValidField);
				Assert.NotNull(keyboardWillShowMethod);
				Assert.NotNull(clearKeyboardStateMethod);

				cacheValidField!.SetValue(child, true);
				using var keyboardFrame = NSValue.FromCGRect(new CGRect(0, 400, 400, 400));
				using var userInfo = NSDictionary.FromObjectAndKey(keyboardFrame, UIKeyboard.FrameEndUserInfoKey);
				using var notification = NSNotification.FromName(UIKeyboard.WillShowNotification, parent, userInfo);

				keyboardWillShowMethod!.Invoke(parent, [notification]);
				Assert.False((bool)cacheValidField.GetValue(child)!);

				cacheValidField.SetValue(child, true);
				clearKeyboardStateMethod!.Invoke(parent, null);
				Assert.False((bool)cacheValidField.GetValue(child)!);
			});
		}

		// Insets seen with Display Zoom "Larger Text" (#36975): every other layout pass, UIKit
		// reports the same half-pixel inset with floating-point noise.
		[Theory]
		[InlineData(1, 26.5, 26.499999999999886)]
		[InlineData(3, 27.166666666666668, 27.166666666666554)]
		public async Task SafeAreaNoiseAtHalfPixelDoesNotRemeasure(double scale, double inset, double noisyInset)
		{
			await InvokeOnMainThreadAsync(() =>
			{
#pragma warning disable CA1422 // A scene-less window is enough to give the view an effective scale.
				using var window = new UIWindow();
#pragma warning restore CA1422
				var layout = new MeasureCountingSafeAreaLayout();
				using var view = new NoisySafeAreaView { View = layout, CrossPlatformLayout = layout, Frame = new CGRect(0, 0, 320, 120) };
				window.AddSubview(view);
				view.ContentScaleFactor = (nfloat)scale;

				// A bounded stand-in for the run loop, which would otherwise lay out forever
				for (int pass = 0; pass < 10; pass++)
				{
					view.BottomInset = pass % 2 == 0 ? inset : noisyInset;
					view.SafeAreaInsetsDidChange();
					view.LayoutSubviews();
				}

				// Only the first pass, which starts applying the inset, needs a measure
				Assert.Equal(1, layout.MeasureCount);
			});
		}

		class NoisySafeAreaView : MauiView
		{
			public double BottomInset { get; set; }
			public override UIEdgeInsets SafeAreaInsets => new(0, 0, (nfloat)BottomInset, 0);
		}

		class MeasureCountingSafeAreaLayout : StubBase, ICrossPlatformLayout, ISafeAreaView2
		{
			public int MeasureCount { get; private set; }
			public bool HasExplicitSafeAreaEdges => true;
			public Thickness SafeAreaInsets { private get; set; }
			public SafeAreaRegions GetSafeAreaRegionsForEdge(int edge) => SafeAreaRegions.Container;

			public Graphics.Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
			{
				MeasureCount++;
				return new Graphics.Size(widthConstraint, heightConstraint);
			}

			public Graphics.Size CrossPlatformArrange(Graphics.Rect bounds) => bounds.Size;
		}

		[Fact]
		public async Task NonUIControlDisablesUserInteractionWhenIsEnabledFalse()
		{
			var view = new StubBase();
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.True(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}

		[Fact]
		public async Task NonUIControlKeepsInputTransparentAfterIsEnabledToggles()
		{
			var view = new StubBase { InputTransparent = true };
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(handler.PlatformView.UserInteractionEnabled);
			});
		}

		[Theory]
		[InlineData(false, true)]
		[InlineData(true, false)]
		public async Task ContainerUserInteractionTracksIsEnabledAndInputTransparent(bool inputTransparent, bool expectedWhenEnabled)
		{
			var view = new StubBase
			{
				Clip = new PathShapeStub(),
				InputTransparent = inputTransparent
			};
			var handler = await CreateHandlerAsync(view);

			await InvokeOnMainThreadAsync(() =>
			{
				var containerView = Assert.IsType<WrapperView>(handler.ContainerView);

				Assert.Equal(expectedWhenEnabled, containerView.UserInteractionEnabled);

				view.IsEnabled = false;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.False(containerView.UserInteractionEnabled);

				view.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				Assert.Equal(expectedWhenEnabled, containerView.UserInteractionEnabled);
			});
		}
	}
}