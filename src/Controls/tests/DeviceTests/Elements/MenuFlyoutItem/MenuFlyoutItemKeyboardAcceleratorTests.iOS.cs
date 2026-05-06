#if MACCATALYST
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public class MenuFlyoutItemKeyboardAcceleratorTests : ControlsHandlerTestBase
	{
		// Regression tests for https://github.com/dotnet/maui/issues/35279
		// UIKeyCommand.Create requires lowercase alphabetic input strings. Passing an uppercase
		// letter (e.g. "S") causes Mac Catalyst to silently reject the UIKeyCommand, making the
		// entire parent UIMenu non-functional. The fix normalises single-letter keys to lowercase
		// inside CreateMenuItemKeyCommand before they reach UIKeyCommand.Create.

		[Theory(DisplayName = "Alphabetic key is normalised to lowercase before UIKeyCommand.Create")]
		[InlineData("S", "s")]
		[InlineData("A", "a")]
		[InlineData("Z", "z")]
		public void AlphabeticKeyIsNormalisedToLowercase(string inputKey, string expectedInput)
		{
			try
			{
				var item = new MenuFlyoutItem { Text = "Test Item" };
				item.KeyboardAccelerators.Add(new KeyboardAccelerator
				{
					Key = inputKey,
					Modifiers = KeyboardAcceleratorModifiers.Cmd | KeyboardAcceleratorModifiers.Shift
				});

				var uiMenuElement = item.CreateMenuItem(MauiContext);
				var keyCommand = Assert.IsType<UIKeyCommand>(uiMenuElement);

				Assert.Equal(expectedInput, keyCommand.Input);
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		[Fact(DisplayName = "Cmd+Shift+S (the issue #35279 repro case) produces Input='s'")]
		public void CmdShiftSProducesLowercaseInput()
		{
			try
			{
				var item = new MenuFlyoutItem { Text = "Save As" };
				item.KeyboardAccelerators.Add(new KeyboardAccelerator
				{
					Key = "S", // uppercase — the root cause of issue #35279
					Modifiers = KeyboardAcceleratorModifiers.Cmd | KeyboardAcceleratorModifiers.Shift
				});

				var uiMenuElement = item.CreateMenuItem(MauiContext);
				var keyCommand = Assert.IsType<UIKeyCommand>(uiMenuElement);

				Assert.Equal("s", keyCommand.Input);
				Assert.Equal(UIKeyModifierFlags.Command | UIKeyModifierFlags.Shift, keyCommand.ModifierFlags);
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}
	}
}
#endif
