using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class EntryCoreGalleryPage : CoreGalleryPage<Entry>
	{
		public EntryCoreGalleryPage()
		{
		}

		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var completedContainer = new EventViewContainer<Entry>(Test.Entry.Completed, new Entry { Placeholder = "Completed" });
			completedContainer.View.Completed += (sender, args) => completedContainer.EventFired();

			var placeholderContainer = new ViewContainer<Entry>(Test.Entry.Placeholder, new Entry { Placeholder = "Placeholder" });
			var keyboardContainer = new ViewContainer<Entry>(Test.InputView.Keyboard, new Entry { Keyboard = Keyboard.Numeric });
			var isPasswordContainer = new ViewContainer<Entry>(Test.Entry.IsPassword, new Entry { IsPassword = true });
			var textContainer = new ViewContainer<Entry>(Test.Entry.Text, new Entry { Text = "Hi, I am Text" });

			var textChangedContainer = new EventViewContainer<Entry>(Test.Entry.TextChanged, new Entry());
			textChangedContainer.View.TextChanged += (sender, args) => textChangedContainer.EventFired();

			var textFontAttributesContainer = new ViewContainer<Entry>(Test.Entry.FontAttributes, new Entry { Text = "I have italic text", FontAttributes = FontAttributes.Italic });
			var textFamilyContainer1 = new ViewContainer<Entry>(Test.Entry.FontFamily, new Entry { Text = "I have Comic Sans text in Win & Android", FontFamily = "Comic Sans MS" });
			var textFamilyContainer2 = new ViewContainer<Entry>(Test.Entry.FontFamily, new Entry { Text = "I have bold Chalkboard text in iOS", FontFamily = "ChalkboardSE-Regular", FontAttributes = FontAttributes.Bold });
			var textFontSizeContainer = new ViewContainer<Entry>(Test.Entry.FontSize, new Entry { Text = "I have default size text" });
			var textFontSizeDefaultContainer = new ViewContainer<Entry>(Test.Entry.FontSize, new Entry { Text = "I also have default size text" });
			textFontSizeDefaultContainer.View.FontSize = Device.GetNamedSize(NamedSize.Default, textFontSizeDefaultContainer.View);
			var textFontSizeLargeContainer = new ViewContainer<Entry>(Test.Entry.FontSize, new Entry { Text = "I have size 48 (huge) text", FontSize = 48 });

			var textColorContainer = new ViewContainer<Entry>(Test.Entry.TextColor, new Entry { Text = "Hi, I should be red", TextColor = Color.Red });

			var xAlignCenterContainer = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentCenter,
				new Entry { Text = "Should be centered", HorizontalTextAlignment = TextAlignment.Center });
			var xAlignEndContainer = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentEnd,
				new Entry { Text = "Should be aligned end", HorizontalTextAlignment = TextAlignment.End });
			var xAlignStartContainer = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentStart,
				new Entry { Text = "Should be aligned start", HorizontalTextAlignment = TextAlignment.Start });

			var xAlignPlaceholderCenter = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentPlaceholderCenter,
				new Entry { Placeholder = "Should be centered", HorizontalTextAlignment = TextAlignment.Center });
			var xAlignPlaceholderEnd = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentPlaceholderEnd,
				new Entry { Placeholder = "Should be aligned end", HorizontalTextAlignment = TextAlignment.End });
			var xAlignPlaceholderStart = new ViewContainer<Entry>(Test.Entry.HorizontalTextAlignmentPlaceholderStart,
				new Entry { Placeholder = "Should be aligned start", HorizontalTextAlignment = TextAlignment.Start });

			var yAlignCenterContainer = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentCenter,
				new Entry { Text = "Should be centered!", VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Color.Pink, HeightRequest = 100 });
			var yAlignEndContainer = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentEnd,
				new Entry { Text = "Should be aligned end!", VerticalTextAlignment = TextAlignment.End, BackgroundColor = Color.Pink, HeightRequest = 100 });
			var yAlignStartContainer = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentStart,
				new Entry { Text = "Should be aligned start!", VerticalTextAlignment = TextAlignment.Start, BackgroundColor = Color.Pink, HeightRequest = 100 });

			var yAlignPlaceholderCenter = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentPlaceholderCenter,
				new Entry { Placeholder = "Should be centered!", VerticalTextAlignment = TextAlignment.Center, BackgroundColor = Color.Pink, HeightRequest = 100 });
			var yAlignPlaceholderEnd = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentPlaceholderEnd,
				new Entry { Placeholder = "Should be aligned end!", VerticalTextAlignment = TextAlignment.End, BackgroundColor = Color.Pink, HeightRequest = 100 });
			var yAlignPlaceholderStart = new ViewContainer<Entry>(Test.Entry.VerticalTextAlignmentPlaceholderStart,
				new Entry { Placeholder = "Should be aligned start!", VerticalTextAlignment = TextAlignment.Start, BackgroundColor = Color.Pink, HeightRequest = 100 });

			var placeholderColorContainer = new ViewContainer<Entry>(Test.Entry.PlaceholderColor,
				new Entry { Placeholder = "Hi, I should be red", PlaceholderColor = Color.Red });

			var textColorDisabledContainer = new ViewContainer<Entry>(Test.Entry.TextDisabledColor,
				new Entry { IsEnabled = false, Text = "I should be the default disabled color", TextColor = Color.Red });

			var placeholderColorDisabledContainer = new ViewContainer<Entry>(Test.Entry.PlaceholderDisabledColor,
				new Entry
				{
					IsEnabled = false,
					Placeholder = "I should be the default placeholder disabled color",
					PlaceholderColor = Color.Red
				});

			var passwordColorContainer = new ViewContainer<Entry>(Test.Entry.PasswordColor,
				new Entry { IsPassword = true, Text = "12345", TextColor = Color.Red });

			var maxLengthContainer = new ViewContainer<Entry>(Test.InputView.MaxLength, new Entry { MaxLength = 3 });

			var readOnlyContainer = new ViewContainer<Entry>(Test.Entry.IsReadOnly, new Entry { Text = "This is read-only Entry", IsReadOnly = true });
			var isPasswordInputScopeContainer = new ViewContainer<Entry>(Test.Entry.IsPasswordNumeric, new Entry { Keyboard = Keyboard.Numeric });
			var switchPasswordButton = new Button
			{
				Text = "Toggle IsPassword"
			};
			var switchNumericButton = new Button
			{
				Text = "Toggle numeric"
			};
			switchPasswordButton.Clicked += (o, a) =>
			{
				isPasswordInputScopeContainer.View.IsPassword = !isPasswordInputScopeContainer.View.IsPassword;
			};
			switchNumericButton.Clicked += (o, a) =>
			{
				isPasswordInputScopeContainer.View.Keyboard = isPasswordInputScopeContainer.View.Keyboard == Keyboard.Numeric ? Keyboard.Default : Keyboard.Numeric;
			};
			isPasswordInputScopeContainer.ContainerLayout.Children.Add(switchPasswordButton);
			isPasswordInputScopeContainer.ContainerLayout.Children.Add(switchNumericButton);

			var switchClearBtnVisibilityBtn = new Button { Text = "Toggle ClearButtonVisibility" };
			var clearBtnModelContainer = new ViewContainer<Entry>(Test.Entry.ClearButtonVisibility,
				new Entry { Text = "I should have clear button visible", ClearButtonVisibility = ClearButtonVisibility.WhileEditing });
			switchClearBtnVisibilityBtn.Clicked += (o, a) =>
				clearBtnModelContainer.View.ClearButtonVisibility = clearBtnModelContainer.View.ClearButtonVisibility == ClearButtonVisibility.Never ? ClearButtonVisibility.WhileEditing : ClearButtonVisibility.Never;
			clearBtnModelContainer.ContainerLayout.Children.Add(switchClearBtnVisibilityBtn);

			Add(isPasswordContainer);
			Add(completedContainer);
			Add(placeholderContainer);
			Add(keyboardContainer);
			Add(textContainer);
			Add(textChangedContainer);
			Add(textColorContainer);
			Add(xAlignPlaceholderCenter);
			Add(xAlignCenterContainer);
			Add(xAlignPlaceholderEnd);
			Add(xAlignEndContainer);
			Add(xAlignPlaceholderStart);
			Add(xAlignStartContainer);
			Add(yAlignPlaceholderCenter);
			Add(yAlignCenterContainer);
			Add(yAlignPlaceholderEnd);
			Add(yAlignEndContainer);
			Add(yAlignPlaceholderStart);
			Add(yAlignStartContainer);
			Add(textFontAttributesContainer);
			Add(textFamilyContainer1);
			Add(textFamilyContainer2);
			Add(textFontSizeContainer);
			Add(textFontSizeDefaultContainer);
			Add(textFontSizeLargeContainer);
			Add(placeholderColorContainer);
			Add(textColorDisabledContainer);
			Add(placeholderColorDisabledContainer);
			Add(passwordColorContainer);
			Add(maxLengthContainer);
			Add(readOnlyContainer);
			Add(isPasswordInputScopeContainer);
			Add(clearBtnModelContainer);
		}
	}
}