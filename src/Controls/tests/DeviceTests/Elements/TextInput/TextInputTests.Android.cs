using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	[Collection(RunInNewWindowCollection)]
	public partial class TextInputTests
	{
		[Theory]
		[InlineData(typeof(Editor))]
		[InlineData(typeof(Entry))]
		[InlineData(typeof(SearchBar))]
		public async Task ShowsKeyboardOnFocus(Type controlType)
		{
			SetupBuilder();
			var textInput = (View)Activator.CreateInstance(controlType);

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = (IPlatformViewHandler)CreateHandler(textInput);
				var platformView = handler.PlatformView;

				await platformView.AttachAndRun(async () =>
				{
					textInput.Focus();
					await AssertionExtensions.WaitForFocused(platformView, 2000, $"WaitForFocused failed on Line 29 after first focus on {controlType}");
					await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed on Line 30 after first focus on {controlType}");

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(platformView, message: $"HideKeyboardForView failed on Line 33 after first keyboard show on {controlType}");
					textInput.Focus();
					await AssertionExtensions.WaitForFocused(platformView, message: $"WaitForFocused failed on Line 35 after second focus on {controlType}");
					await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed on Line 36 after second focus on {controlType}");
					await AssertionExtensions.HideKeyboardForView(platformView, message: $"HideKeyboardForView failed on Line 37 after second keyboard show on {controlType}");
				});
			});
		}
	}
}
