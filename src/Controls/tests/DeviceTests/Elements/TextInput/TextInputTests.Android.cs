using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
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
			textInput.HeightRequest = 100;
			textInput.WidthRequest = 100;
			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = (IPlatformViewHandler)CreateHandler(textInput);
				var platformView = handler.PlatformView;

				await platformView.AttachAndRun(async () =>
				{
					try
					{
						await AssertionExtensions.HideKeyboardForView(platformView, message: $"Make sure keyboard starts out closed {controlType}");
						textInput.Focus();
						await AssertionExtensions.WaitForFocused(platformView, message: $"WaitForFocused failed after first focus on {controlType}");
						await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed after first focus on {controlType}");

						// Test that keyboard reappears when refocusing on an already focused TextInput control
						await AssertionExtensions.HideKeyboardForView(platformView, message: $"HideKeyboardForView failed after first keyboard show on {controlType}");
						textInput.Focus();
						await AssertionExtensions.WaitForFocused(platformView, message: $"WaitForFocused failed after second focus on {controlType}");
						await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed after second focus on {controlType}");
					}
					finally
					{
						await AssertionExtensions.HideKeyboardForView(platformView, message: $"HideKeyboardForView after test has finished {controlType}");
					}
				});
			});
		}
	}
}
