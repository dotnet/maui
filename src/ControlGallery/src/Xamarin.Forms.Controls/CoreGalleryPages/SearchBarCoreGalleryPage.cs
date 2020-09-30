using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class SearchBarCoreGalleryPage : CoreGalleryPage<SearchBar>
	{
		// TODO
		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);
			var placeholderContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceHolder, new SearchBar { Placeholder = "Placeholder" });

			var searchButtonPressedContainer = new EventViewContainer<SearchBar>(Test.SearchBar.SearchButtonPressed, new SearchBar { });
			searchButtonPressedContainer.View.SearchButtonPressed += (sender, args) => searchButtonPressedContainer.EventFired();

			var searchCommandContainer = new ViewContainer<SearchBar>(Test.SearchBar.SearchCommand,
				new SearchBar
				{
					SearchCommand = new Command(async () => await DisplayAlert("Search command", "Fired", "Ok"))
				}
			);

			var textContainer = new ViewContainer<SearchBar>(Test.SearchBar.Text, new SearchBar { Text = "I am text" });

			var textChangedContainer = new EventViewContainer<SearchBar>(Test.SearchBar.TextChanged, new SearchBar { Placeholder = "I am text changed" });
			textChangedContainer.View.TextChanged += (sender, args) => textChangedContainer.EventFired();

			var cancelButtonColor = new ViewContainer<SearchBar>(
				Test.SearchBar.CancelButtonColor,
				new SearchBar
				{
					Placeholder = "Should have a red cancel button",
					CancelButtonColor = Color.Red
				}
			);

			var textFontAttributesContainer = new ViewContainer<SearchBar>(Test.SearchBar.FontAttributes, new SearchBar { Text = "I have italic text", FontAttributes = FontAttributes.Italic });
			var textFamilyContainer1 = new ViewContainer<SearchBar>(Test.SearchBar.FontFamily, new SearchBar { Text = "I have Comic Sans text in Win & Android", FontFamily = "Comic Sans MS" });
			var textFamilyContainer2 = new ViewContainer<SearchBar>(Test.SearchBar.FontFamily, new SearchBar { Text = "I have bold Chalkboard text in iOS", FontFamily = "ChalkboardSE-Regular", FontAttributes = FontAttributes.Bold });
			var textFontSizeContainer = new ViewContainer<SearchBar>(Test.SearchBar.FontSize, new SearchBar { Text = "I have default size text" });
			var textFontSizeDefaultContainer = new ViewContainer<SearchBar>(Test.SearchBar.FontSize, new SearchBar { Text = "I also have default size text" });
			textFontSizeDefaultContainer.View.FontSize = Device.GetNamedSize(NamedSize.Default, textFontSizeDefaultContainer.View);
			var textFontSizeLargeContainer = new ViewContainer<SearchBar>(Test.SearchBar.FontSize, new SearchBar { Text = "I have size 48 (huge) text", FontSize = 48 });

			var textAlignmentStartContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextAlignmentStart,
				new SearchBar { Text = "I should be at the start", HorizontalTextAlignment = TextAlignment.Start });
			var textAlignmentCenterContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextAlignmentCenter,
				new SearchBar { Text = "I should be centered", HorizontalTextAlignment = TextAlignment.Center });
			var textAlignmentEndContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextAlignmentEnd,
				new SearchBar { Text = "I should be at the end", HorizontalTextAlignment = TextAlignment.End });

			var textVerticalAlignmentStartContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextVerticalAlignmentStart,
				new SearchBar { Text = "I should be at the start", VerticalTextAlignment = TextAlignment.Start });
			var textVerticalAlignmentCenterContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextVerticalAlignmentCenter,
				new SearchBar { Text = "I should be centered", VerticalTextAlignment = TextAlignment.Center });
			var textVerticalAlignmentEndContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextVerticalAlignmentEnd,
				new SearchBar { Text = "I should be at the end", VerticalTextAlignment = TextAlignment.End });

			var placeholderAlignmentStartContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderAlignmentStart,
				new SearchBar { Placeholder = "I should be at the start", HorizontalTextAlignment = TextAlignment.Start });
			var placeholderAlignmentCenterContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderAlignmentCenter,
				new SearchBar { Placeholder = "I should be centered", HorizontalTextAlignment = TextAlignment.Center });
			var placeholderAlignmentEndContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderAlignmentEnd,
				new SearchBar { Placeholder = "I should be at the end", HorizontalTextAlignment = TextAlignment.End });

			var placeholderVerticalAlignmentStartContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderVerticalAlignmentStart,
				new SearchBar { Placeholder = "I should be at the start", VerticalTextAlignment = TextAlignment.Start });
			var placeholderVerticalAlignmentCenterContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderVerticalAlignmentCenter,
				new SearchBar { Placeholder = "I should be centered", VerticalTextAlignment = TextAlignment.Center });
			var placeholderVerticalAlignmentEndContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderVerticalAlignmentEnd,
				new SearchBar { Placeholder = "I should be at the end", VerticalTextAlignment = TextAlignment.End });

			var textColorContainer = new ViewContainer<SearchBar>(Test.SearchBar.TextColor,
				new SearchBar { Text = "I should be red", TextColor = Color.Red });

			var placeholderColorContainer = new ViewContainer<SearchBar>(Test.SearchBar.PlaceholderColor,
				new SearchBar { Placeholder = "I should be red", PlaceholderColor = Color.Red });

			var keyboardContainer = new ViewContainer<SearchBar>(Test.InputView.Keyboard,
				new SearchBar { Keyboard = Keyboard.Numeric });

			var maxLengthContainer = new ViewContainer<SearchBar>(Test.InputView.MaxLength,
				new SearchBar { MaxLength = 3 });

			Add(placeholderContainer);
			Add(searchButtonPressedContainer);
			Add(searchCommandContainer);
			Add(textContainer);
			Add(textChangedContainer);
			Add(textFontAttributesContainer);
			Add(textFamilyContainer1);
			Add(textFamilyContainer2);
			Add(textFontSizeContainer);
			Add(textFontSizeDefaultContainer);
			Add(textFontSizeLargeContainer);
			Add(cancelButtonColor);
			Add(textAlignmentStartContainer);
			Add(textAlignmentCenterContainer);
			Add(textAlignmentEndContainer);
			Add(placeholderAlignmentStartContainer);
			Add(placeholderAlignmentCenterContainer);
			Add(placeholderAlignmentEndContainer);
			Add(textVerticalAlignmentStartContainer);
			Add(textVerticalAlignmentCenterContainer);
			Add(textVerticalAlignmentEndContainer);
			Add(placeholderVerticalAlignmentStartContainer);
			Add(placeholderVerticalAlignmentCenterContainer);
			Add(placeholderVerticalAlignmentEndContainer);
			Add(textColorContainer);
			Add(placeholderColorContainer);
			Add(keyboardContainer);
			Add(maxLengthContainer);
		}
	}
}