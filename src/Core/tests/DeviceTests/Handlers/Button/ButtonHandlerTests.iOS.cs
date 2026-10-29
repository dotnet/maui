using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ButtonHandlerTests
	{
		[Fact]
		public async Task GradientCornerRadiusUpdatesWhenResized()
		{
			var button = new ButtonStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Blue),
				CornerRadius = 100,
				Width = 50,
				Height = 50,
				DesiredSize = new Microsoft.Maui.Graphics.Size(50, 50)
			};

			var radii = await GetValueAsync(button, handler =>
			{
				double GetGradientCornerRadius()
				{
					var backgroundLayer = handler.PlatformView.Layer.Sublayers?.FirstOrDefault(layer => layer.Name == "MauiBackgroundLayer");
					Assert.NotNull(backgroundLayer);
					return backgroundLayer.CornerRadius;
				}

				handler.PlatformArrange(new Microsoft.Maui.Graphics.Rect(0, 0, 50, 50));
				var initialRadius = GetGradientCornerRadius();

				var resizedBounds = new Microsoft.Maui.Graphics.Rect(0, 0, 40, 40);
				handler.PlatformArrange(resizedBounds);
				var resizedRadius = GetGradientCornerRadius();
				handler.PlatformArrange(resizedBounds);

				return (initialRadius, resizedRadius, sameSizeRadius: GetGradientCornerRadius());
			});

			Assert.Equal(25, radii.initialRadius, Precision);
			Assert.Equal(20, radii.resizedRadius, Precision);
			Assert.Equal(20, radii.sameSizeRadius, Precision);
		}

		[Fact]
		public async Task CornerRadiusUpdatesWhenResized()
		{
			var button = new ButtonStub
			{
				CornerRadius = 100,
				Width = 50,
				Height = 50,
				DesiredSize = new Microsoft.Maui.Graphics.Size(50, 50)
			};

			var radii = await GetValueAsync(button, handler =>
			{
				handler.PlatformArrange(new Microsoft.Maui.Graphics.Rect(0, 0, 50, 50));
				var initialRadius = (double)handler.PlatformView.Layer.CornerRadius;

				var resizedBounds = new Microsoft.Maui.Graphics.Rect(0, 0, 40, 40);
				handler.PlatformArrange(resizedBounds);
				var resizedRadius = (double)handler.PlatformView.Layer.CornerRadius;
				handler.PlatformArrange(resizedBounds);

				return (initialRadius, resizedRadius, sameSizeRadius: (double)handler.PlatformView.Layer.CornerRadius);
			});

			Assert.Equal(25, radii.initialRadius, Precision);
			Assert.Equal(20, radii.resizedRadius, Precision);
			Assert.Equal(20, radii.sameSizeRadius, Precision);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;

			var button = new ButtonStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingAndTextColorInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;
			var color = Colors.HotPink;

			var button = new ButtonStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				TextColor = color,
				Text = originalText
			};

			var values = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			var colorvalues = await GetValueAsync(button, (handler) =>
			{
				return new
				{
					ViewValue = button.TextColor,
					PlatformViewValue = GetNativeTextColor(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);

			Assert.Equal(color, colorvalues.ViewValue);
			Assert.Equal(color, colorvalues.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing updates size Correctly")]
		public async Task CharacterSpacingUpdatesSizeCorrectly()
		{
			string originalText = "Loren ipsum";

			var button = new ButtonStub()
			{
				CharacterSpacing = 4,
				Text = originalText
			};

			double newCharacterSpacing = 14;

			var handler = await CreateHandlerAsync(button);

			await InvokeOnMainThreadAsync(async () =>
			{
				await handler.PlatformView.AttachAndRun(() =>
				{
					double previousWidth = handler.PlatformView.Bounds.Width;

					button.CharacterSpacing = newCharacterSpacing;
					handler.UpdateValue(nameof(ILabel.CharacterSpacing));

					var platformCharacterSpacing = GetNativeCharacterSpacing(handler);
					Assert.Equal(newCharacterSpacing, platformCharacterSpacing);

					handler.PlatformView.SizeToFit();
					double newWidth = handler.PlatformView.Bounds.Width;
					Assert.True(newWidth > previousWidth);
				});
			});
		}

		[Fact(DisplayName = "Default Accessibility Traits Don't Change")]
		[InlineData()]
		public async Task ValidateDefaultAccessibilityTraits()
		{
			var view = new ButtonStub();
			var trait = await GetValueAsync((IView)view,
				handler =>
				{
					// Accessibility Traits don't initialize until after
					// a UIView is added to the visual hierarchy so we are just 
					// initializing here and then validating that the value doesn't get cleared

					handler.PlatformView.AccessibilityTraits = UIAccessibilityTrait.Button;
					view.Semantics.Hint = "Test Hint";
					view.Handler.UpdateValue("Semantics");
					return handler.PlatformView.AccessibilityTraits;
				});

			Assert.Equal(UIAccessibilityTrait.Button, trait);
		}

		bool ImageSourceLoaded(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView.ImageView.Image != null;

		UIButton GetNativeButton(ButtonHandler buttonHandler) =>
			(UIButton)buttonHandler.PlatformView;

		string GetNativeText(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).CurrentTitle;

		Color GetNativeTextColor(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).CurrentTitleColor.ToColor();

#pragma warning disable CA1416, CA1422
		UIEdgeInsets GetNativePadding(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).ContentEdgeInsets;
#pragma warning restore CA1416, CA1422

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}

		double GetNativeCharacterSpacing(ButtonHandler buttonHandler)
		{
			var button = GetNativeButton(buttonHandler);

			var attributedText = button.GetAttributedTitle(UIControlState.Normal);

			return attributedText.GetCharacterSpacing();
		}

		UILineBreakMode GetNativeLineBreakMode(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).TitleLabel.LineBreakMode;
	}
}