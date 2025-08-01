#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WTextAlignment = Microsoft.UI.Xaml.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		static TextBox GetPlatformControl(EntryHandler handler) =>
			handler.PlatformView;

		static Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		Task<float> GetPlatformOpacity(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return (float)nativeView.Opacity;
			});
		}

		static void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		static int GetPlatformCursorPosition(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionStart;

		static int GetPlatformSelectionLength(EntryHandler entryHandler) =>
			GetPlatformControl(entryHandler).SelectionLength;

		Task<bool> GetPlatformIsVisible(EntryHandler entryHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformControl(entryHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}
		//src/Compatibility/Core/tests/WinUI/FlowDirectionTests.cs
		[Theory]
		[InlineData(true, FlowDirection.LeftToRight, WTextAlignment.Left)]
		[InlineData(true, FlowDirection.RightToLeft, WTextAlignment.Left)]
		[InlineData(false, FlowDirection.LeftToRight, WTextAlignment.Left)]
		[InlineData(false, FlowDirection.RightToLeft, WTextAlignment.Left)]
		[Description("The Entry's text alignment should match the expected alignment when FlowDirection is applied explicitly or implicitly.")]
		public async Task EntryAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection, WTextAlignment expectedAlignment)
		{
			var entry = new Entry { Text = "Checking flow direction", HorizontalTextAlignment = TextAlignment.Start };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = entry };

			if (isExplicit)
			{
				entry.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}

			var handler = await CreateHandlerAsync<EntryHandler>(entry);
			var nativeAlignment = await contentPage.Dispatcher.DispatchAsync(() =>
			{
				var textField = GetPlatformControl(handler);
				return textField.TextAlignment;
			});

			Assert.Equal(expectedAlignment, nativeAlignment);
		}

		[Fact]
		[Description("Password entry should refresh obfuscation when identical text is pasted")]
		public async Task PasswordEntryObfuscatesIdenticalPastedText()
		{
			var entry = new Entry
			{
				Text = "password123",
				IsPassword = true
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<EntryHandler>(entry);
				var platformControl = GetPlatformControl(handler);
				var passwordTextBox = platformControl as Microsoft.Maui.Platform.MauiPasswordTextBox;

				Assert.NotNull(passwordTextBox);
				Assert.True(passwordTextBox.IsPassword);

				// Verify the text is initially obfuscated
				Assert.Equal(new string('●', entry.Text.Length), passwordTextBox.Text);

				// Simulate pasting the same text by setting the same text value
				passwordTextBox.Text = "password123";

				// This is equivalent to what happens when a user pastes identical text via Ctrl+V
				passwordTextBox.GetType().GetMethod("UpdatePasswordIfNeeded",
					System.Reflection.BindingFlags.NonPublic |
					System.Reflection.BindingFlags.Instance)?.Invoke(passwordTextBox, null);


				// Verify the text is still properly obfuscated after the "paste" operation
				Assert.Equal(new string('●', entry.Text.Length), passwordTextBox.Text);
			});
		}
	}
}
