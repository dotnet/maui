using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class EntryHandlerTests
	{
		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("Times New Roman")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var entry = new EntryStub
			{
				Text = "Test",
				Font = Font.OfSize(family, 10)
			};

			var (services, nativeFont) = await GetValueAsync(entry, handler => (handler.Services, GetNativeEntry(handler).Font));

			var fontManager = services.GetRequiredService<IFontManager>();

			var expectedNativeFont = fontManager.GetFont(Font.OfSize(family, 0.0));

			Assert.Equal(expectedNativeFont.FamilyName, nativeFont.FamilyName);
			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
			else
				Assert.NotEqual(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
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

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.HorizontalTextAlignment,
					NativeViewValue = GetNativeTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.NativeViewValue.AssertHasFlag(expectedValue);
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

			UIReturnKeyType expectedValue = UIReturnKeyType.Next;

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.ReturnType,
					NativeViewValue = GetNativeReturnType(handler)
				};
			});

			Assert.Equal(xplatReturnType, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Some Test Text";
			var xplatCharacterSpacing = 4;

			var entry = new EntryStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(entry, (handler) =>
			{
				return new
				{
					ViewValue = entry.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.NativeViewValue);
		}

		double GetNativeCharacterSpacing(EntryHandler entryHandler)
		{
			var entry = GetNativeEntry(entryHandler);
			return entry.AttributedText.GetCharacterSpacing();
		}

		UITextField GetNativeEntry(EntryHandler entryHandler) =>
			(UITextField)entryHandler.View;

		string GetNativeText(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Text;

		void SetNativeText(EntryHandler entryHandler, string text) =>
			GetNativeEntry(entryHandler).Text = text;

		Color GetNativeTextColor(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextColor.ToColor();

		bool GetNativeIsPassword(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).SecureTextEntry;

		string GetNativePlaceholder(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Placeholder;

		bool GetNativeIsTextPredictionEnabled(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).AutocorrectionType == UITextAutocorrectionType.Yes;

		bool GetNativeIsReadOnly(EntryHandler entryHandler) =>
			!GetNativeEntry(entryHandler).UserInteractionEnabled;

		double GetNativeUnscaledFontSize(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Font.PointSize;

		bool GetNativeIsBold(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		bool GetNativeIsItalic(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);

		bool GetNativeClearButtonVisibility(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ClearButtonMode == UITextFieldViewMode.WhileEditing;

		UITextAlignment GetNativeTextAlignment(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).TextAlignment;

		UIReturnKeyType GetNativeReturnType(EntryHandler entryHandler) =>
			GetNativeEntry(entryHandler).ReturnKeyType;
	}
}