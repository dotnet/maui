using System.Linq;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using global::Android.Text;
using global::Android.Text.Method;
using global::Android.Views.InputMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Theory(DisplayName = "Validates Keyboard updates correctly using IsPassword")]
		[InlineData(nameof(Keyboard.Text), false)]
		[InlineData(nameof(Keyboard.Numeric), true)]
		public async Task ValidateNumericKeyboardUpdatesCorrectlyWithIsPassword(string keyboardName, bool expected)
		{
			var layout = new LayoutStub();

			var keyboard = (Keyboard)typeof(Keyboard).GetProperty(keyboardName).GetValue(null);

			var entry = new EntryStub()
			{
				IsPassword = false,
				Keyboard = keyboard,
				Text = "Password"
			};

			var button = new ButtonStub
			{
				Text = "Change IsPassword"
			};

			layout.Add(entry);
			layout.Add(button);

			var clicked = false;

			button.Clicked += delegate
			{
				entry.IsPassword = !entry.IsPassword;
				clicked = true;
			};

			await PerformClick(button); // IsPassword = true
			Assert.True(clicked);

			await PerformClick(button); // IsPassword = false

			await ValidatePropertyInitValue(entry, () => expected, GetNativeIsNumericKeyboard, expected);
		}

		[Fact(DisplayName = "Entry Background Updates Correctly")]
		public async Task BackgroundUpdatesCorrectly()
		{
			var expected = Colors.Red;

			var layout = new LayoutStub();

			var entry = new EntryStub
			{
				Text = "Entry",
				Background = new SolidPaintStub(Colors.Yellow),
			};

			var button = new ButtonStub
			{
				Text = "Change Background"
			};

			layout.Add(entry);
			layout.Add(button);

			var clicked = false;

			button.Clicked += delegate
			{
				entry.Background = new SolidPaintStub(expected);
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);

			await ValidateHasColor(entry, expected);
		}

		[Theory(DisplayName = "ClearButton Color Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task ClearButtonColorInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var entry = new EntryStub()
			{
				Text = "Test",
				TextColor = expected,
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing
			};

			entry.Focus();

			await ValidateHasColor(entry, expected);
		}

		[Fact(DisplayName = "Padding is the same after background changes")]
		public async Task PaddingDoesntChangeAfterBackground()
		{
			var paddingLeft = 0;
			var paddingTop = 0;
			var paddingRight = 0;
			var paddingBottom = 0;

			var entry = new EntryStub();

			entry.PropertyMapperOverrides = new PropertyMapper<IEntry, IEntryHandler>(EntryHandler.Mapper)
			{
				["MyCustomization"] = (handler, view) =>
				{
					handler.PlatformView.SetPadding(paddingLeft, paddingTop, paddingRight, paddingBottom);
				}
			};

			var paddingLeftNative = await GetValueAsync(entry, h => h.PlatformView.PaddingLeft);
			var paddingRightNative = await GetValueAsync(entry, h => h.PlatformView.PaddingRight);
			var paddingTopNative = await GetValueAsync(entry, h => h.PlatformView.PaddingTop);
			var paddingBottomNative = await GetValueAsync(entry, h => h.PlatformView.PaddingBottom);
			Assert.Equal(paddingLeft, paddingLeftNative);
			Assert.Equal(paddingTop, paddingTopNative);
			Assert.Equal(paddingRight, paddingRightNative);
			Assert.Equal(paddingBottom, paddingBottomNative);
			entry.Background = new SolidPaint(Colors.Red);
			var paddingLeftNativeAfter = await GetValueAsync(entry, h => h.PlatformView.PaddingLeft);
			var paddingRightNativeAfter = await GetValueAsync(entry, h => h.PlatformView.PaddingRight);
			var paddingTopNativeAfter = await GetValueAsync(entry, h => h.PlatformView.PaddingTop);
			var paddingBottomNativeAfter = await GetValueAsync(entry, h => h.PlatformView.PaddingBottom);
			Assert.Equal(paddingLeft, paddingLeftNative);
			Assert.Equal(paddingTop, paddingTopNativeAfter);
			Assert.Equal(paddingRight, paddingRightNativeAfter);
			Assert.Equal(paddingBottom, paddingBottomNativeAfter);
		}


		[Fact(DisplayName = "PlaceholderColor Initializes Correctly")]
		public async Task PlaceholderColorInitializesCorrectly()
		{
			var entry = new EntryStub()
			{
				Placeholder = "Test",
				PlaceholderColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(entry, () => entry.PlaceholderColor, GetNativePlaceholderColor, entry.PlaceholderColor);
		}

		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new EntryStub()
			{
				Text = "Test",
				ReturnType = xplatReturnType
			};

			ImeAction expectedValue = ImeAction.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					PlatformViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var entry = new EntryStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			global::Android.Views.TextAlignment expectedValue = global::Android.Views.TextAlignment.ViewEnd;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		static AppCompatEditText GetNativeEntry(EntryHandler entryHandler) =>
			entryHandler.PlatformView;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		internal static void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		internal static int GetCursorStartPosition(EntryHandler entryHandler)
		{
			var control = GetNativeEntry(entryHandler);
			return control.SelectionStart;
		}

		internal static void UpdateCursorStartPosition(EntryHandler entryHandler, int position)
		{
			var control = GetNativeEntry(entryHandler);
			control.SetSelection(position);
		}

		Color GetNativeTextColor(EntryHandler entryHandler)
		{
			int currentTextColorInt = GetNativeEntry(entryHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		bool GetNativeIsPassword(EntryHandler entryHandler)
		{
			var inputType = GetNativeEntry(entryHandler).InputType;
			return inputType.HasFlag(InputTypes.TextVariationPassword) || inputType.HasFlag(InputTypes.NumberVariationPassword);
		}

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).InputType.HasFlag(InputTypes.TextFlagAutoCorrect);

		bool GetNativeIsSpellCheckEnabled(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).InputType.HasFlag(InputTypes.TextFlagNoSuggestions);

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Hint;

		Color GetNativePlaceholderColor(EntryHandler entryHandler)
		{
			int currentHintTextColor = GetNativeEntry(entryHandler).CurrentHintTextColor;
			AColor currentPlaceholderColor = new AColor(currentHintTextColor);
			return currentPlaceholderColor.ToColor();
		}

		bool GetNativeIsReadOnly(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			return !editText.Focusable && !editText.FocusableInTouchMode;
		}

		bool GetNativeIsNumericKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return editText.KeyListener is NumberKeyListener
				&& (inputTypes.HasFlag(InputTypes.NumberFlagDecimal) && inputTypes.HasFlag(InputTypes.ClassNumber) && inputTypes.HasFlag(InputTypes.NumberFlagSigned));
		}

		bool GetNativeIsChatKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		bool GetNativeIsEmailKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationEmailAddress));
		}

		bool GetNativeIsTelephoneKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassPhone);
		}

		bool GetNativeIsUrlKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationUri);
		}

		bool GetNativeIsTextKeyboard(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);
			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		global::Android.Views.TextAlignment GetNativeHorizontalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		global::Android.Views.GravityFlags GetNativeVerticalTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Gravity & global::Android.Views.GravityFlags.VerticalGravityMask;

		global::Android.Views.GravityFlags GetNativeVerticalTextAlignment(TextAlignment textAlignment) =>
			textAlignment.ToVerticalGravityFlags();

		ImeAction GetNativeReturnType(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ImeOptions;

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler)
		{
			var nativeEntry = GetNativeEntry(entryHandler);
			var unfocusedDrawables = nativeEntry.GetCompoundDrawables();

			bool compoundsValidWhenUnfocused = !unfocusedDrawables.Any(a => a != null);

			// This will display 'X' drawable.
			nativeEntry.RequestFocus();

			var focusedDrawables = nativeEntry.GetCompoundDrawables();

			// Index 2 for FlowDirection.LeftToRight.
			bool compoundsValidWhenFocused = focusedDrawables.Length == 4 && focusedDrawables[2] != null;

			return compoundsValidWhenFocused && compoundsValidWhenUnfocused;
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var entry = new EntryStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Some Test Text"
			};

			float expectedValue = entry.CharacterSpacing.ToEm();

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		double GetNativeCharacterSpacing(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
			{
				return editText.LetterSpacing;
			}

			return -1;
		}

		int GetNativeCursorPosition(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
				return editText.SelectionEnd;

			return -1;
		}

		int GetNativeSelectionLength(EntryHandler entryHandler)
		{
			var editText = GetNativeEntry(entryHandler);

			if (editText != null)
				return editText.SelectionEnd - editText.SelectionStart;

			return -1;
		}

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler<ButtonHandler>(button)).PerformClick();
			});
		}
	}
}
