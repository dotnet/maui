using System.Threading.Tasks;
using Android.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	public class SearchHandlerInputTypeTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(InputTypes.ClassText | InputTypes.TextVariationUri, "ABC")]
		[InlineData(InputTypes.ClassText | InputTypes.TextVariationPassword, "AbC")]
		[InlineData(InputTypes.ClassNumber | InputTypes.NumberVariationPassword, "AbC")]
		public async Task TextTransformRespectsInputClassAndPasswordVariation(InputTypes inputType, string expected)
		{
			var entryHandler = await CreateHandlerAsync<EntryHandler>(new Entry());
			var searchHandler = new SearchHandler { TextTransform = TextTransform.Uppercase };
			var editText = entryHandler.PlatformView;

			await InvokeOnMainThreadAsync(() => editText.InputType = inputType);

			var (_, newText) = await InvokeOnMainThreadAsync(() =>
				Microsoft.Maui.Controls.Platform.EditTextExtensions.GetTexts(editText, "AbC", searchHandler));

			Assert.Equal(expected, newText);
		}

		[Fact]
		public async Task KeyboardSettingsUsePasswordAwareTextTransform()
		{
			var urlEntry = new Entry
			{
				Keyboard = Keyboard.Url,
				IsPassword = false,
				TextTransform = TextTransform.Uppercase
			};
			var urlHandler = await CreateHandlerAsync<EntryHandler>(urlEntry);
			await InvokeOnMainThreadAsync(() =>
			{
				urlEntry.Text = "AbC";
			});
			Assert.Equal("ABC", await InvokeOnMainThreadAsync(() => urlHandler.PlatformView.Text));

			var textPasswordEntry = new Entry
			{
				Keyboard = Keyboard.Text,
				IsPassword = true,
				TextTransform = TextTransform.Uppercase
			};
			var textPasswordHandler = await CreateHandlerAsync<EntryHandler>(textPasswordEntry);
			await InvokeOnMainThreadAsync(() =>
			{
				textPasswordEntry.Text = "AbC";
			});
			Assert.Equal("AbC", await InvokeOnMainThreadAsync(() => textPasswordHandler.PlatformView.Text));

			var numericPasswordEntry = new Entry
			{
				Keyboard = Keyboard.Numeric,
				IsPassword = true,
				TextTransform = TextTransform.Uppercase
			};
			var numericPasswordHandler = await CreateHandlerAsync<EntryHandler>(numericPasswordEntry);
			await InvokeOnMainThreadAsync(() =>
			{
				numericPasswordEntry.Text = "AbC";
			});
			Assert.Equal("AbC", await InvokeOnMainThreadAsync(() => numericPasswordHandler.PlatformView.Text));
		}
	}
}
