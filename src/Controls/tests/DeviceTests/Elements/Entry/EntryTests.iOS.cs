using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.TestCases;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryTests
	{
		UITextField GetPlatformControl(EntryHandler handler) =>
			(UITextField)handler.PlatformView;

		Task<string> GetPlatformText(EntryHandler handler)
		{
			return InvokeOnMainThreadAsync(() => GetPlatformControl(handler).Text);
		}

		void SetPlatformText(EntryHandler entryHandler, string text) =>
			GetPlatformControl(entryHandler).Text = text;

		int GetPlatformCursorPosition(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.BeginningOfDocument, textField.SelectedTextRange.Start);

			return -1;
		}

		int GetPlatformSelectionLength(EntryHandler entryHandler)
		{
			var textField = GetPlatformControl(entryHandler);

			if (textField != null && textField.SelectedTextRange != null)
				return (int)textField.GetOffsetFromPosition(textField.SelectedTextRange.Start, textField.SelectedTextRange.End);

			return -1;
		}

		[Category(TestCategory.Entry)]
		[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
		public partial class EntryTestsWithWindow : ControlsHandlerTestBase
		{
			[Theory]
			[ClassData(typeof(ControlsPageTypesTestCases))]
			public async Task NextMovesToNextEntry(string page)
			{
				EnsureHandlerCreated(builder =>
				{
					ControlsPageTypesTestCases.Setup(builder);
					builder.ConfigureMauiHandlers(handlers =>
					{
						handlers.AddHandler(typeof(Entry), typeof(EntryHandler));
					});
				});

				var entry1 = new Entry
				{
					Text = "Entry 1",
					ReturnType = ReturnType.Next
				};

				var entry2 = new Entry
				{
					Text = "Entry 2",
					ReturnType = ReturnType.Next
				};

				ContentPage contentPage = new ContentPage()
				{
					Content = new VerticalStackLayout()
					{
						entry1,
						entry2
					}
				};

				Page rootPage = ControlsPageTypesTestCases.CreatePageType(page, contentPage);
				Page hostPage = new ContentPage();

				await CreateHandlerAndAddToWindow(hostPage, async () =>
				{
					await hostPage.Navigation.PushModalAsync(rootPage);
					KeyboardAutoManager.GoToNextResponderOrResign(entry1.ToPlatform());
					await AssertionExtensions.Wait(() => entry2.IsFocused);
					Assert.True(entry2.IsFocused);
				});
			}
		}
	}
}