using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		[Theory(DisplayName = "Gradient Background Initializes Correctly")]
		[InlineData(0xFF0000)]
		[InlineData(0x00FF00)]
		[InlineData(0x0000FF)]
		public async Task GradientBackgroundInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var brush = new LinearGradientPaintStub(Colors.Black, expected);

			var searchBar = new SearchBarStub
			{
				Background = brush,
			};

			await ValidateHasColor(searchBar, expected);
		}

		[Fact]
		public async Task ShouldShowCancelButtonToggles()
		{
			var searchBarStub = new SearchBarStub();

			await InvokeOnMainThreadAsync(() =>
			{
				var searchBar = CreateHandler(searchBarStub).PlatformView;

				Assert.False(searchBar.ShowsCancelButton);

				searchBarStub.Text = "additional text";
				searchBarStub.Handler.UpdateValue(nameof(ISearchBar.Text));
				Assert.True(searchBar.ShowsCancelButton);

				searchBarStub.Text = "";
				searchBarStub.Handler.UpdateValue(nameof(ISearchBar.Text));
				Assert.False(searchBar.ShowsCancelButton);
			});
		}

		[Fact(DisplayName = "Horizontal TextAlignment Updates Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			UITextAlignment expectedValue = UITextAlignment.Right;

			var values = await GetValueAsync(searchBarStub, (handler) =>
			{
				return new
				{
					ViewValue = searchBarStub.HorizontalTextAlignment,
					PlatformViewValue = GetNativeHorizontalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatHorizontalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Updates Correctly")]
		public async Task VerticalTextAlignmentInitializesCorrectly()
		{
			var xplatVerticalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				VerticalTextAlignment = xplatVerticalTextAlignment
			};

			UIControlContentVerticalAlignment expectedValue = UIControlContentVerticalAlignment.Bottom;

			var values = await GetValueAsync(searchBarStub, (handler) =>
			{
				return new
				{
					ViewValue = searchBarStub.VerticalTextAlignment,
					PlatformViewValue = GetNativeVerticalTextAlignment(handler)
				};
			});

			Assert.Equal(xplatVerticalTextAlignment, values.ViewValue);
			values.PlatformViewValue.AssertHasFlag(expectedValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			string originalText = "Test";
			var xplatCharacterSpacing = 4;

			var slider = new SearchBarStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = originalText
			};

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		double GetInputFieldHeight(SearchBarHandler searchBarHandler)
		{
			return GetNativeSearchBar(searchBarHandler).Bounds.Height;
		}

		static UISearchBar GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			(UISearchBar)searchBarHandler.PlatformView;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Text;

		static void SetNativeText(SearchBarHandler searchBarHandler, string text) =>
			GetNativeSearchBar(searchBarHandler).Text = text;

		static int GetCursorStartPosition(SearchBarHandler searchBarHandler)
		{
			var control = searchBarHandler.QueryEditor;
			return (int)control.GetOffsetFromPosition(control.BeginningOfDocument, control.SelectedTextRange.Start);
		}

		static void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
		{
			var control = searchBarHandler.QueryEditor;
			var endPosition = control.GetPosition(control.BeginningOfDocument, position);
			control.SelectedTextRange = control.GetTextRange(endPosition, endPosition);
		}

		Color GetNativeTextColor(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return Colors.Transparent;

			return textField.TextColor.ToColor();
		}

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Placeholder;

		UITextAlignment GetNativeHorizontalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return UITextAlignment.Left;

			return textField.TextAlignment;
		}

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return UIControlContentVerticalAlignment.Center;

			return textField.VerticalAlignment;
		}

		double GetNativeCharacterSpacing(SearchBarHandler searchBarHandler)
		{
			var searchBar = GetNativeSearchBar(searchBarHandler);
			var textField = searchBar.FindDescendantView<UITextField>();

			return textField.AttributedText.GetCharacterSpacing();
		}

		double GetNativeUnscaledFontSize(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);
			var textField = uiSearchBar.FindDescendantView<UITextField>();

			if (textField == null)
				return -1;

			return textField.Font.PointSize;
		}

		bool GetNativeIsReadOnly(SearchBarHandler searchBarHandler)
		{
			var uiSearchBar = GetNativeSearchBar(searchBarHandler);

			return !uiSearchBar.UserInteractionEnabled;
		}

		Task ValidateHasColor(ISearchBar searchBar, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeSearchBar = GetNativeSearchBar(CreateHandler(searchBar));
				action?.Invoke();
				nativeSearchBar.AssertContainsColor(color);
			});
		}
	}
}