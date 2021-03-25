using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public partial class EntryHandlerTests : HandlerTestBase<EntryHandler, EntryStub>
	{
		[Fact(DisplayName = "Text Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Test"
			};

			await ValidatePropertyInitValue(entry, () => entry.Text, GetNativeText, entry.Text);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				TextColor = Color.Yellow
			};

			await ValidatePropertyInitValue(entry, () => entry.TextColor, GetNativeTextColor, entry.TextColor);
		}

		[Theory(DisplayName = "IsPassword Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsPasswordInitializesCorrectly(bool isPassword)
		{
			var entry = new EntryStub()
			{
				IsPassword = isPassword
			};

			await ValidatePropertyInitValue(entry, () => entry.IsPassword, GetNativeIsPassword, isPassword);
		}

		[Fact(DisplayName = "Placeholder Initializes Correctly")]
		public async Task PlaceholderInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Placeholder"
			};

			await ValidatePropertyInitValue(entry, () => entry.Placeholder, GetNativePlaceholder, entry.Placeholder);
		}

		[Theory(DisplayName = "Is Text Prediction Enabled")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledCorrectly(bool isEnabled)
		{
			var entry = new EntryStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await ValidatePropertyInitValue(entry, () => entry.IsTextPredictionEnabled, GetNativeIsTextPredictionEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsPassword Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsPasswordUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var entry = new EntryStub();

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.IsPassword),
				GetNativeIsPassword,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "TextColor Updates Correctly")]
		[InlineData(0xFF0000, 0x0000FF)]
		[InlineData(0x0000FF, 0xFF0000)]
		public async Task TextColorUpdatesCorrectly(uint setValue, uint unsetValue)
		{
			var entry = new EntryStub();

			var setColor = Color.FromUint(setValue);
			var unsetColor = Color.FromUint(unsetValue);

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.TextColor),
				GetNativeTextColor,
				setColor,
				unsetColor);
		}

		[Theory(DisplayName = "Text Updates Correctly")]
		[InlineData(null, null)]
		[InlineData(null, "Hello")]
		[InlineData("Hello", null)]
		[InlineData("Hello", "Goodbye")]
		public async Task TextUpdatesCorrectly(string setValue, string unsetValue)
		{
			var entry = new EntryStub();

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.Text),
				h =>
				{
					var n = GetNativeText(h);
					if (string.IsNullOrEmpty(n))
						n = null; // native platforms may not upport null text
					return n;
				},
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsTextPredictionEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var entry = new EntryStub();

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.IsTextPredictionEnabled),
				GetNativeIsTextPredictionEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsReadOnly Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsReadOnlyUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var entry = new EntryStub();

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.IsReadOnly),
				GetNativeIsReadOnly,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(entry, () => entry.Font.FontSize, GetNativeUnscaledFontSize, entry.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontAttributes.None, false, false)]
		[InlineData(FontAttributes.Bold, true, false)]
		[InlineData(FontAttributes.Italic, false, true)]
		[InlineData(FontAttributes.Bold | FontAttributes.Italic, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontAttributes attributes, bool isBold, bool isItalic)
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				Font = Font.OfSize("Arial", 10).WithAttributes(attributes)
			};

			await ValidatePropertyInitValue(entry, () => entry.Font.FontAttributes.HasFlag(FontAttributes.Bold), GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(entry, () => entry.Font.FontAttributes.HasFlag(FontAttributes.Italic), GetNativeIsItalic, isItalic);
		}

		[Theory(DisplayName = "Validates clear button visibility.")]
		[InlineData(ClearButtonVisibility.WhileEditing, true)]
		[InlineData(ClearButtonVisibility.Never, false)]
		public async Task ValidateClearButtonVisibility(ClearButtonVisibility clearButtonVisibility, bool expected)
		{
			var entryStub = new EntryStub()
			{
				ClearButtonVisibility = clearButtonVisibility,
				Text = "Test text input.",
				FlowDirection = FlowDirection.LeftToRight
			};

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeClearButtonVisibility, expected);
		}

		[Theory(DisplayName = "TextChanged Events Fire Correctly")]
		// null/empty
		[InlineData(null, null, false)]
		[InlineData(null, "", false)]
		[InlineData("", null, false)]
		[InlineData("", "", false)]
		// whitespace
		[InlineData(null, " ", true)]
		[InlineData("", " ", true)]
		[InlineData(" ", null, true)]
		[InlineData(" ", "", true)]
		[InlineData(" ", " ", false)]
		// text
		[InlineData(null, "Hello", true)]
		[InlineData("", "Hello", true)]
		[InlineData(" ", "Hello", true)]
		[InlineData("Hello", null, true)]
		[InlineData("Hello", "", true)]
		[InlineData("Hello", " ", true)]
		[InlineData("Hello", "Goodbye", true)]
		public async Task TextChangedEventsFireCorrectly(string initialText, string newText, bool eventExpected)
		{
			var entry = new EntryStub
			{
				Text = initialText,
			};

			var eventFiredCount = 0;
			entry.TextChanged += (sender, e) =>
			{
				eventFiredCount++;

				Assert.Equal(initialText, e.OldValue);
				Assert.Equal(newText ?? string.Empty, e.NewValue);
			};

			await SetValueAsync(entry, newText, SetNativeText);

			if (eventExpected)
				Assert.Equal(1, eventFiredCount);
			else
				Assert.Equal(0, eventFiredCount);
		}

		[Theory(DisplayName = "MaxLength Initializes Correctly")]
		[InlineData(2)]
		[InlineData(5)]
		[InlineData(8)]
		[InlineData(10)]
		public async Task MaxLengthInitializesCorrectly(int maxLength)
		{
			const string text = "Lorem ipsum dolor sit amet";
			var expectedText = text.Substring(0, maxLength);

			var entry = new EntryStub()
			{
				MaxLength = maxLength,
				Text = text
			};

			var nativeText = await GetValueAsync(entry, GetNativeText);

			Assert.Equal(expectedText, nativeText);
			Assert.Equal(expectedText, entry.Text);
		}

		[Theory(DisplayName = "MaxLength Clips Native Text Correctly")]
		[InlineData(2)]
		[InlineData(5)]
		[InlineData(8)]
		[InlineData(10)]
		public async Task MaxLengthClipsNativeTextCorrectly(int maxLength)
		{
			const string text = "Lorem ipsum dolor sit amet";
			var expectedText = text.Substring(0, maxLength);

			var entry = new EntryStub()
			{
				MaxLength = maxLength,
			};

			var nativeText = await GetValueAsync(entry, handler =>
			{
				entry.Text = text;

				return GetNativeText(handler);
			});

			Assert.Equal(expectedText, nativeText);
			Assert.Equal(expectedText, entry.Text);
		}
	}
}