#if MACCATALYST
using System.Threading.Tasks;
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

		[Theory(DisplayName = "Single-character key normalization only lowercases uppercase alphabetic input")]
		[InlineData("S", "s")]
		[InlineData("A", "a")]
		[InlineData("Z", "z")]
		[InlineData("s", "s")]
		[InlineData("+", "+")]
		[InlineData("1", "1")]
		public async Task AlphabeticKeyIsNormalisedToLowercase(string inputKey, string expectedInput)
		{
			try
			{
				await InvokeOnMainThreadAsync(() =>
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
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		[Fact(DisplayName = "Cmd+Shift+S (the issue #35279 repro case) produces Input='s'")]
		public async Task CmdShiftSProducesLowercaseInput()
		{
			try
			{
				await InvokeOnMainThreadAsync(() =>
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
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}

		[Fact(DisplayName = "Multi-character key input passes through unchanged")]
		public async Task MultiCharacterKeyPassesThroughUnchanged()
		{
			try
			{
				await InvokeOnMainThreadAsync(() =>
				{
					var item = new MenuFlyoutItem { Text = "Function" };
					item.KeyboardAccelerators.Add(new KeyboardAccelerator
					{
						Key = "F1",
						Modifiers = KeyboardAcceleratorModifiers.Cmd
					});

					var uiMenuElement = item.CreateMenuItem(MauiContext);
					var keyCommand = Assert.IsType<UIKeyCommand>(uiMenuElement);

					Assert.Equal("F1", keyCommand.Input);
				});
			}
			finally
			{
				MenuFlyoutItemHandler.Reset();
			}
		}
	}
}
#endif
