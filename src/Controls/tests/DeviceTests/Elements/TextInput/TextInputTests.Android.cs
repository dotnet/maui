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
					await AssertionExtensions.WaitForFocused(platformView, 3000);
					await AssertionExtensions.WaitForKeyboardToShow(platformView);

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(platformView);
					textInput.Focus();
					await AssertionExtensions.WaitForFocused(platformView, 3000);
					await AssertionExtensions.WaitForKeyboardToShow(platformView);
					await AssertionExtensions.HideKeyboardForView(platformView);
				});
			});

		}
	}
}
