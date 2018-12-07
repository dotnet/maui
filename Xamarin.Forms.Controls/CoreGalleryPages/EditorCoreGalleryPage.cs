using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class EditorCoreGalleryPage : CoreGalleryPage<Editor>
	{
		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var completedContainer = new EventViewContainer<Editor>(Test.Editor.Completed, new Editor());
			completedContainer.View.Completed += (sender, args) => completedContainer.EventFired();

			var textContainer = new ViewContainer<Editor>(Test.Editor.Text, new Editor { Text = "I have text" });

			var textChangedContainer = new EventViewContainer<Editor>(Test.Editor.TextChanged, new Editor());
			textChangedContainer.View.TextChanged += (sender, args) => textChangedContainer.EventFired();
			var placeholderContainer = new ViewContainer<Editor>(Test.Editor.Placeholder, new Editor { Placeholder = "Placeholder" });
			var textFontAttributesContainer = new ViewContainer<Editor>(Test.Editor.FontAttributes, new Editor { Text = "I have italic text", FontAttributes = FontAttributes.Italic });
			var textFamilyContainer1 = new ViewContainer<Editor>(Test.Editor.FontFamily, new Editor { Text = "I have Comic Sans text in Win & Android", FontFamily = "Comic Sans MS" });
			var textFamilyContainer2 = new ViewContainer<Editor>(Test.Editor.FontFamily, new Editor { Text = "I have bold Chalkboard text in iOS", FontFamily = "ChalkboardSE-Regular", FontAttributes = FontAttributes.Bold });
			var textFontSizeContainer = new ViewContainer<Editor>(Test.Editor.FontSize, new Editor { Text = "I have default size text" });
			var textFontSizeDefaultContainer = new ViewContainer<Editor>(Test.Editor.FontSize, new Editor { Text = "I also have default size text" });
			textFontSizeDefaultContainer.View.FontSize = Device.GetNamedSize(NamedSize.Default, textFontSizeDefaultContainer.View);
			var textFontSizeLargeContainer = new ViewContainer<Editor>(Test.Editor.FontSize, new Editor { Text = "I have size 48 (huge) text", FontSize = 48 });

			var textColorContainer = new ViewContainer<Editor>(Test.Editor.TextColor,
				new Editor { Text = "I should have red text", TextColor = Color.Red });

			var textColorDisabledContainer = new ViewContainer<Editor>(Test.Editor.TextColor,
				new Editor { Text = "I should have the default disabled text color", TextColor = Color.Red, IsEnabled = false });

			var keyboardContainer = new ViewContainer<Editor>(Test.InputView.Keyboard,
				new Editor { Keyboard = Keyboard.Numeric });

			var maxLengthContainer = new ViewContainer<Editor>(Test.InputView.MaxLength, new Editor { MaxLength = 3 });

			Add(completedContainer);
			Add(textContainer);
			Add(textChangedContainer);
			Add(placeholderContainer);
			Add(textFontAttributesContainer);
			Add(textFamilyContainer1);
			Add(textFamilyContainer2);
			Add(textFontSizeContainer);
			Add(textFontSizeDefaultContainer);
			Add(textFontSizeLargeContainer);
			Add(textColorContainer);
			Add(textColorDisabledContainer);
			Add(keyboardContainer);
			Add(maxLengthContainer);
		}
	}
}