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

			var attributedText = button.TitleLabel.AttributedText;

			return attributedText.GetCharacterSpacing();
		}

		UILineBreakMode GetNativeLineBreakMode(ButtonHandler buttonHandler) =>
			GetNativeButton(buttonHandler).TitleLabel.LineBreakMode;
	}
}