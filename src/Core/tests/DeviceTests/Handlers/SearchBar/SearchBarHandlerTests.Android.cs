using System;
using System.Linq;
using System.Threading.Tasks;
using Android.Text;
using Android.Text.Method;
using Android.Views.InputMethods;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;
using AColor = Android.Graphics.Color;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SearchBarHandlerTests
	{
		[Fact(DisplayName = "ReturnType Initializes Correctly")]
		public async Task ReturnTypeInitializesCorrectly()
		{
			var xplatReturnType = ReturnType.Next;
			var entry = new SearchBarStub()
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

		[Fact(DisplayName = "PlaceholderColor Initializes Correctly")]
		public async Task PlaceholderColorInitializesCorrectly()
		{
			var searchBar = new SearchBarStub()
			{
				Placeholder = "Test",
				PlaceholderColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(searchBar, () => searchBar.PlaceholderColor, GetNativePlaceholderColor, searchBar.PlaceholderColor);
		}

		[Fact(DisplayName = "Horizontal TextAlignment Initializes Correctly")]
		public async Task HorizontalTextAlignmentInitializesCorrectly()
		{
			var xplatHorizontalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				HorizontalTextAlignment = xplatHorizontalTextAlignment
			};

			Android.Views.TextAlignment expectedValue = Android.Views.TextAlignment.ViewEnd;

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

		[Fact(DisplayName = "Vertical TextAlignment Initializes Correctly")]
		public async Task VerticalTextAlignmentInitializesCorrectly()
		{
			var xplatVerticalTextAlignment = TextAlignment.End;

			var searchBarStub = new SearchBarStub()
			{
				Text = "Test",
				VerticalTextAlignment = xplatVerticalTextAlignment
			};

			Android.Views.GravityFlags expectedValue = Android.Views.GravityFlags.Bottom;

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
			var xplatCharacterSpacing = 4;

			var searchBar = new SearchBarStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Text = "Test"
			};

			float expectedValue = searchBar.CharacterSpacing.ToEm();

			var values = await GetValueAsync(searchBar, (handler) =>
			{
				return new
				{
					ViewValue = searchBar.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Fact]
		public async Task SearchViewHasEditTextChild()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var view = new SearchView(MauiContext.Context);

				var editText = view.GetFirstChildOfType<EditText>();

				Assert.NotNull(editText);
			});
		}

		[Fact]
		public async Task SearchBarTakesFullWidthByDefault()
		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var layout = new LayoutStub()
				{
					Width = 500
				};

				var searchbar = new SearchBarStub
				{
					Text = "My Search Term",
				};

				layout.Add(searchbar);

				var layoutHandler = CreateHandler(layout);
				await layoutHandler.PlatformView.AttachAndRun(() =>
				{
					var layoutSize = layout.Measure(double.PositiveInfinity, double.PositiveInfinity);
					var rect = new Rect(0, 0, layoutSize.Width, layoutSize.Height);
					var searchbarSize = searchbar.Measure(rect.Width, rect.Height);

					Assert.Equal(layoutSize.Width, searchbarSize.Width);
				});
			});
		}

		double GetInputFieldHeight(SearchBarHandler searchBarHandler)
		{
			var control = GetNativeSearchBar(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return MauiContext.Context.FromPixels((double)editText.MeasuredHeight);
		}


		static SearchView GetNativeSearchBar(SearchBarHandler searchBarHandler) =>
			searchBarHandler.PlatformView;

		string GetNativeText(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).Query;

		ImeAction GetNativeReturnType(SearchBarHandler searchBarHandler) =>
			(ImeAction)GetNativeSearchBar(searchBarHandler).ImeOptions;

		static void SetNativeText(SearchBarHandler searchBarHandler, string text) =>
			GetNativeSearchBar(searchBarHandler).SetQuery(text, false);


		static int GetCursorStartPosition(SearchBarHandler searchBarHandler)
		{
			var control = GetNativeSearchBar(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			return editText.SelectionStart;
		}

		static void UpdateCursorStartPosition(SearchBarHandler searchBarHandler, int position)
		{
			var control = GetNativeSearchBar(searchBarHandler);
			var editText = control.GetChildrenOfType<EditText>().FirstOrDefault();
			editText.SetSelection(position);
		}

		Color GetNativeTextColor(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText != null)
			{
				int currentTextColorInt = editText.CurrentTextColor;
				AColor currentTextColor = new AColor(currentTextColorInt);
				return currentTextColor.ToColor();
			}

			return Colors.Transparent;
		}

		Android.Views.TextAlignment GetNativeHorizontalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();
			return editText.TextAlignment;
		}

		Android.Views.GravityFlags GetNativeVerticalTextAlignment(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();
			return editText.Gravity;
		}

		string GetNativePlaceholder(SearchBarHandler searchBarHandler) =>
			GetNativeSearchBar(searchBarHandler).QueryHint;

		Color GetNativePlaceholderColor(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText != null)
			{
				int currentHintTextColor = editText.CurrentHintTextColor;
				AColor currentPlaceholderColorr = new AColor(currentHintTextColor);
				return currentPlaceholderColorr.ToColor();
			}

			return Colors.Transparent;
		}

		double GetNativeCharacterSpacing(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText != null)
			{
				return editText.LetterSpacing;
			}

			return -1;
		}

		Android.Views.TextAlignment GetNativeTextAlignment(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();
			return editText.TextAlignment;
		}

		bool GetNativeIsReadOnly(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			return !editText.Focusable && !editText.FocusableInTouchMode;
		}

		bool GetNativeIsNumericKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return editText.KeyListener is NumberKeyListener
				&& (inputTypes.HasFlag(InputTypes.NumberFlagDecimal) && inputTypes.HasFlag(InputTypes.ClassNumber) && inputTypes.HasFlag(InputTypes.NumberFlagSigned));
		}

		bool GetNativeIsChatKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		bool GetNativeIsEmailKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return (inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationEmailAddress));
		}

		bool GetNativeIsTelephoneKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassPhone);
		}

		bool GetNativeIsUrlKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextVariationUri);
		}

		bool GetNativeIsTextKeyboard(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.ClassText) && inputTypes.HasFlag(InputTypes.TextFlagCapSentences) && inputTypes.HasFlag(InputTypes.TextFlagAutoComplete);
		}

		bool GetNativeIsTextPredictionEnabled(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return inputTypes.HasFlag(InputTypes.TextFlagAutoCorrect);
		}

		bool GetNativeIsSpellCheckEnabled(SearchBarHandler searchBarHandler)
		{
			var searchView = GetNativeSearchBar(searchBarHandler);
			var editText = searchView.GetChildrenOfType<EditText>().First();

			if (editText is null)
				return false;

			var inputTypes = editText.InputType;

			return !inputTypes.HasFlag(InputTypes.TextFlagNoSuggestions);
		}
	}
}