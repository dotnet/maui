using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Entry)]
	public partial class EntryHandlerTests : CoreHandlerTestBase<EntryHandler, EntryStub>
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

		[Fact(DisplayName = "Text Property Initializes Correctly when Keyboard Mapper is Executed Before Text Mapper")]
		public async Task TextInitializesCorrectlyWhenKeyboardIsBeforeText()
		{
			var entry = new EntryStub()
			{
				Text = "Test Text Here"
			};

			CustomEntryHandler.TestMapper = new PropertyMapper<IEntry, IEntryHandler>(EntryHandler.Mapper)
			{
				// this mapper is run first and then the ones in the ctor arg (EntryHandler.Mapper)
				[nameof(IEntry.Keyboard)] = EntryHandler.MapKeyboard
			};

			await ValidatePropertyInitValue<string, CustomEntryHandler>(entry, () => entry.Text, GetNativeText, entry.Text);
		}

		[Fact(DisplayName = "Text Property Initializes Correctly when IsReadOnly Mapper is Executed Before Text Mapper")]
		public async Task TextInitializesCorrectlyWhenIsReadOnlyIsBeforeText()
		{
			var entry = new EntryStub()
			{
				Text = "Test Text Here"
			};

			CustomEntryHandler.TestMapper = new PropertyMapper<IEntry, IEntryHandler>(EntryHandler.Mapper)
			{
				// this mapper is run first and then the ones in the ctor arg (EntryHandler.Mapper)
				[nameof(IEntry.IsReadOnly)] = EntryHandler.MapIsReadOnly
			};

			await ValidatePropertyInitValue<string, CustomEntryHandler>(entry, () => entry.Text, GetNativeText, entry.Text);
		}

		[Fact(DisplayName = "Text Property Initializes Correctly when IsPassword Mapper is Executed Before Text Mapper")]
		public async Task TextInitializesCorrectlyWhenIsPasswordIsBeforeText()
		{
			var entry = new EntryStub()
			{
				Text = "Test Text Here"
			};

			CustomEntryHandler.TestMapper = new PropertyMapper<IEntry, IEntryHandler>(EntryHandler.Mapper)
			{
				// this mapper is run first and then the ones in the ctor arg (EntryHandler.Mapper)
				[nameof(IEntry.IsPassword)] = EntryHandler.MapIsPassword
			};

			await ValidatePropertyInitValue<string, CustomEntryHandler>(entry, () => entry.Text, GetNativeText, entry.Text);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				TextColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(entry, () => entry.TextColor, GetNativeTextColor, entry.TextColor);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var entry = new EntryStub()
			{
				Text = "Test",
				TextColor = null
			};

			await CreateHandlerAsync(entry);
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
				Placeholder = "Placeholder"
			};

			await ValidatePropertyInitValue(entry, () => entry.Placeholder, GetNativePlaceholder, "Placeholder");
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
		[InlineData(0xFFFF0000, 0xFF0000FF)]
		[InlineData(0xFF0000FF, 0xFFFF0000)]
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

		[Theory(DisplayName = "IsTextPredictionEnabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsTextPredictionEnabledInitializesCorrectly(bool isEnabled)
		{
			var entry = new EntryStub()
			{
				IsTextPredictionEnabled = isEnabled
			};

			await AttachAndRun(entry, async (entryHandler) =>
			{
				await AssertionExtensions.Wait(() => entryHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyInitValue(entry, () => entry.IsTextPredictionEnabled, GetNativeIsTextPredictionEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsSpellCheckEnabled Initializes Correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsSpellCheckEnabledInitializesCorrectly(bool isEnabled)
		{
			var entry = new EntryStub()
			{
				IsSpellCheckEnabled = isEnabled
			};

			await AttachAndRun(entry, async (entryHandler) =>
			{
				await AssertionExtensions.Wait(() => entryHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyInitValue(entry, () => entry.IsSpellCheckEnabled, GetNativeIsSpellCheckEnabled, isEnabled);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsTextPredictionEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var entry = new EntryStub()
			{
				IsTextPredictionEnabled = setValue
			};

			await AttachAndRun(entry, async (entryHandler) =>
			{
				await AssertionExtensions.Wait(() => entryHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.IsTextPredictionEnabled),
				GetNativeIsTextPredictionEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsSpellCheckEnabled Updates Correctly")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task IsSpellCheckEnabledUpdatesCorrectly(bool setValue, bool unsetValue)
		{
			var entry = new EntryStub()
			{
				IsSpellCheckEnabled = setValue
			};

			await AttachAndRun(entry, async (entryHandler) =>
			{
				await AssertionExtensions.Wait(() => entryHandler.PlatformView.IsLoaded());
			});

			await ValidatePropertyUpdatesValue(
				entry,
				nameof(IEntry.IsSpellCheckEnabled),
				GetNativeIsSpellCheckEnabled,
				setValue,
				unsetValue);
		}

		[Theory(DisplayName = "IsTextPredictionEnabled differs from IsSpellCheckEnabled")]
		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		public async Task TextPredictionDiffersFromSpellChecking(bool textPredictionValue, bool spellCheckValue)
		{
			// Test to prevent: https://github.com/dotnet/maui/issues/8558
			var areValuesEqual = textPredictionValue == spellCheckValue;

			var entry = new EntryStub()
			{
				IsTextPredictionEnabled = textPredictionValue,
				IsSpellCheckEnabled = spellCheckValue
			};

			await AttachAndRun(entry, async (entryHandler) =>
			{
				await AssertionExtensions.Wait(() => entryHandler.PlatformView.IsLoaded());
			});

			var nativeTextPrediction = await GetValueAsync(entry, GetNativeIsTextPredictionEnabled);
			var nativeSpellChecking = await GetValueAsync(entry, GetNativeIsSpellCheckEnabled);

			Assert.Equal(areValuesEqual, (nativeTextPrediction == nativeSpellChecking));
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

		[Theory(DisplayName = "Validates Numeric Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), true)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateNumericKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsNumericKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Email Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), true)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateEmailKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsEmailKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Telephone Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), true)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateTelephoneKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsTelephoneKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Url Keyboard")]
		[InlineData(nameof(Keyboard.Chat), false)]
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Url), true)]
		public async Task ValidateUrlKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsUrlKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Text Keyboard")]
#if ANDROID || IOS || MACCATALYST
		// Android text and Chat keyboards are the same
		[InlineData(nameof(Keyboard.Chat), true)]
#else
		[InlineData(nameof(Keyboard.Chat), false)]
#endif
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
		[InlineData(nameof(Keyboard.Text), true)]
		[InlineData(nameof(Keyboard.Url), false)]
#if WINDOWS
		// The Text keyboard is the default one on Windows
		[InlineData(nameof(Keyboard.Default), true)]
		// Plain is the same as the Default keyboard on Windows
		[InlineData(nameof(Keyboard.Plain), true)]
#elif IOS || MACCATALYST
		// On ios the text and default keyboards are the same
		[InlineData(nameof(Keyboard.Default), true)]
		[InlineData(nameof(Keyboard.Plain), false)]
#else
		[InlineData(nameof(Keyboard.Default), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
#endif
		public async Task ValidateTextKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsTextKeyboard, expected);
		}

		[Theory(DisplayName = "Validates Chat Keyboard")]
		[InlineData(nameof(Keyboard.Chat), true)]
#if IOS || MACCATALYST
		// On iOS the default and chat keyboard are the same
		[InlineData(nameof(Keyboard.Default), true)]
#else
		[InlineData(nameof(Keyboard.Default), false)]
#endif
		[InlineData(nameof(Keyboard.Email), false)]
		[InlineData(nameof(Keyboard.Numeric), false)]
		[InlineData(nameof(Keyboard.Plain), false)]
		[InlineData(nameof(Keyboard.Telephone), false)]
#if ANDROID || IOS || MACCATALYST
		// Android & iOS text and Chat keyboards are the same
		[InlineData(nameof(Keyboard.Text), true)]
#else
		[InlineData(nameof(Keyboard.Text), false)]
#endif
		[InlineData(nameof(Keyboard.Url), false)]
		public async Task ValidateChatKeyboard(string keyboardName, bool expected)
		{
			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entryStub = new EntryStub() { Keyboard = keyboard };

			await ValidatePropertyInitValue(entryStub, () => expected, GetNativeIsChatKeyboard, expected);
		}

		[Theory(DisplayName = "MaxLength Initializes Correctly"
#if WINDOWS
			, Skip = "https://github.com/dotnet/maui/issues/7939"
#endif
			)]
		[InlineData(0)]
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

			var platformText = await GetValueAsync(entry, GetNativeText);

			Assert.Equal(expectedText, platformText);
			Assert.Equal(expectedText, entry.Text);
		}

		[Fact(DisplayName = "Negative MaxLength Does Not Clip")]
		public async Task NegativeMaxLengthDoesNotClip()
		{
			const string text = "Lorem ipsum dolor sit amet";

			var entry = new EntryStub()
			{
				MaxLength = -1,
			};

			var platformText = await GetValueAsync(entry, handler =>
			{
				entry.Text = text;

				return GetNativeText(handler);
			});

			Assert.Equal(text, platformText);
			Assert.Equal(text, entry.Text);
		}

		[Theory(DisplayName = "MaxLength Clips Native Text Correctly"
#if WINDOWS
			, Skip = "https://github.com/dotnet/maui/issues/7939"
#endif
		)]
		[InlineData(0)]
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

			var platformText = await GetValueAsync(entry, handler =>
			{
				entry.Text = text;

				return GetNativeText(handler);
			});

			Assert.Equal(expectedText, platformText);
			Assert.Equal(expectedText, entry.Text);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect CharacterSpacing")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectCharacterSpacing(double initialSize, double newSize)
		{
			var entry = new EntryStub
			{
				Text = "This is TEXT!",
				CharacterSpacing = 5,
				Font = Font.SystemFontOfSize(initialSize)
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeCharacterSpacing,
				nameof(IEntry.Font),
				() => entry.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect CharacterSpacing")]
		[InlineData("Short", "Longer Text")]
		[InlineData("Long thext here", "Short")]
		public async Task TextDoesNotAffectCharacterSpacing(string initialText, string newText)
		{
			var entry = new EntryStub
			{
				Text = initialText,
				CharacterSpacing = 5,
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeCharacterSpacing,
				nameof(IEntry.Text),
				() => entry.Text = newText);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect HorizontalTextAlignment")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
		{
			var entry = new EntryStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				Font = Font.SystemFontOfSize(initialSize),
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeHorizontalTextAlignment,
				nameof(IEntry.Font),
				() => entry.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
		[InlineData("Short", "Longer Text")]
		[InlineData("Long thext here", "Short")]
		public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
		{
			var entry = new EntryStub
			{
				Text = initialText,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeHorizontalTextAlignment,
				nameof(IEntry.Text),
				() => entry.Text = newText);
		}

		[Theory(DisplayName = "Updating MaxLength Does Not Affect HorizontalTextAlignment")]
		[InlineData(5, 20)]
		[InlineData(20, 5)]
		public async Task MaxLengthDoesNotAffectHorizontalTextAlignment(int initialSize, int newSize)
		{
			var entry = new EntryStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				MaxLength = initialSize,
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeHorizontalTextAlignment,
				nameof(IEntry.MaxLength),
				() => entry.MaxLength = newSize);
		}

		[Theory(DisplayName = "Updating CharacterSpacing Does Not Affect HorizontalTextAlignment")]
		[InlineData(1, 5)]
		[InlineData(5, 1)]
		public async Task CharacterSpacingDoesNotAffectHorizontalTextAlignment(int initialSize, int newSize)
		{
			var entry = new EntryStub
			{
				Text = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				CharacterSpacing = initialSize,
			};

			await ValidateUnrelatedPropertyUnaffected(
				entry,
				GetNativeHorizontalTextAlignment,
				nameof(IEntry.CharacterSpacing),
				() => entry.CharacterSpacing = newSize);
		}

		[Theory(DisplayName = "Vertical TextAlignment Initializes Correctly")]
		[InlineData(TextAlignment.Start)]
		[InlineData(TextAlignment.Center)]
		[InlineData(TextAlignment.End)]
		public async Task VerticalTextAlignmentInitializesCorrectly(TextAlignment textAlignment)
		{
			var entry = new EntryStub
			{
				VerticalTextAlignment = textAlignment
			};

			var platformAlignment = GetNativeVerticalTextAlignment(textAlignment);

			var values = await AttachAndRun(entry, (handler) =>
					new
					{
						ViewValue = entry.VerticalTextAlignment,
						PlatformViewValue = GetNativeVerticalTextAlignment(handler)
					});

			Assert.Equal(textAlignment, values.ViewValue);
			Assert.Equal(platformAlignment, values.PlatformViewValue);
		}

#if ANDROID
		[Fact]
		public async Task NextMovesToNextEntrySuccessfully()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<EntryStub, EntryHandler>();
				});
			});

			var layout = new VerticalStackLayoutStub();

			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next
			};

			layout.Add(entry1);
			layout.Add(entry2);

			layout.Width = 100;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var contentViewHandler = CreateHandler<LayoutHandler>(layout);
				await contentViewHandler.PlatformView.AttachAndRun(async () =>
				{
					await entry1.SendKeyboardReturnType(ReturnType.Next);
					await entry2.WaitForFocused();
					Assert.True(entry2.IsFocused);
				});
			});
		}

		[Fact]
		public async Task DoneClosesKeyboard()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handler =>
				{
					handler.AddHandler<VerticalStackLayoutStub, LayoutHandler>();
					handler.AddHandler<EntryStub, EntryHandler>();
				});
			});

			var layout = new VerticalStackLayoutStub();

			var entry1 = new EntryStub
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Done
			};

			var entry2 = new EntryStub
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Done
			};

			layout.Add(entry1);
			layout.Add(entry2);

			layout.Width = 100;
			layout.Height = 150;

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<LayoutHandler>(layout);
				await handler.PlatformView.AttachAndRun(async () =>
				{
					await entry1.SendKeyboardReturnType(ReturnType.Done);
					await entry1.WaitForKeyboardToHide();
				});
			});
		}
#endif

		[Category(TestCategory.Entry)]
		public class EntryTextStyleTests : TextStyleHandlerTests<EntryHandler, EntryStub>
		{
		}

		[Category(TestCategory.Entry)]
		public class EntryFocusTests : FocusHandlerTests<EntryHandler, EntryStub, VerticalStackLayoutStub>
		{
		}

		[Category(TestCategory.Entry)]
		public class EntryTextInputTests : TextInputHandlerTests<EntryHandler, EntryStub>
		{
			protected override void SetNativeText(EntryHandler entryHandler, string text) =>
				EntryHandlerTests.SetNativeText(entryHandler, text);

			protected override int GetCursorStartPosition(EntryHandler entryHandler) =>
				EntryHandlerTests.GetCursorStartPosition(entryHandler);

			protected override void UpdateCursorStartPosition(EntryHandler entryHandler, int position) =>
				EntryHandlerTests.UpdateCursorStartPosition(entryHandler, position);
		}

		class CustomEntryHandler : EntryHandler
		{
			// make a copy of the Core mappers because we don't want any Controls changes or to override us
			public static PropertyMapper<IEntry, IEntryHandler> TestMapper = new(Mapper);
			public static CommandMapper<IEntry, IEntryHandler> TestCommandMapper = new(CommandMapper);

			// make sure to use our mappers
			public CustomEntryHandler()
				: base(TestMapper, TestCommandMapper)
			{
			}
		}
	}
}
