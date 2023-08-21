// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public abstract partial class TextInputFocusTests<THandler, TView> : ControlsHandlerTestBase
		where THandler : class, IViewHandler, IPlatformViewHandler, new()
		where TView : VisualElement, ITextInput, new()
	{
		[Fact]
		public async Task ShowsKeyboardOnFocus()
		{
			var textInput = new TView() as VisualElement;
			textInput.HeightRequest = 100;
			textInput.WidthRequest = 100;

			await AttachAndRun<THandler>(textInput, async (handler) =>
			{
				try
				{
					var platformView = handler.PlatformView;
					await AssertionExtensions.HideKeyboardForView(textInput, message: $"Make sure keyboard starts out closed");
					textInput.Focus();
					await AssertionExtensions.WaitForFocused(platformView, message: $"WaitForFocused failed after first focus");
					await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed after first focus");

					// Test that keyboard reappears when refocusing on an already focused TextInput control
					await AssertionExtensions.HideKeyboardForView(textInput, message: $"HideKeyboardForView failed after first keyboard show");
					textInput.Focus();
					await AssertionExtensions.WaitForFocused(platformView, message: $"WaitForFocused failed after second focus");
					await AssertionExtensions.WaitForKeyboardToShow(platformView, message: $"WaitForKeyboardToShow failed after second focus");
				}
				finally
				{
					await AssertionExtensions.HideKeyboardForView(textInput, message: $"HideKeyboardForView after test has finished");
				}
			});
		}
	}
}
