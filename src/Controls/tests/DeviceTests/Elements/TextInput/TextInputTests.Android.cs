using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TextInputTests
	{
		[Theory]
		//[InlineData(typeof(Editor))]
		[InlineData(typeof(Entry))]
		//[InlineData(typeof(SearchBar))]
		public async Task ShowsKeyboardOnFocus(Type controlType)
		{
			SetupBuilder();
			var textInput = (View)Activator.CreateInstance(controlType);

			await InvokeOnMainThreadAsync(async () =>
			{
				// Test passes with this line
				//var handler = CreateHandler<EntryHandler>(textInput);

				// Test fails with this line
				var handler = textInput.ToHandler(MauiContext);

				await handler.PlatformView.AttachAndRun(async () =>
				{
					textInput.Focus();
						
					await AssertionExtensions.WaitForKeyboardToShow(handler.PlatformView);

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(handler.PlatformView);
					await AssertionExtensions.WaitForKeyboardToHide(handler.PlatformView);
					textInput.Focus();
					await AssertionExtensions.WaitForKeyboardToShow(handler.PlatformView);
			});
		});

		}
	}
}
