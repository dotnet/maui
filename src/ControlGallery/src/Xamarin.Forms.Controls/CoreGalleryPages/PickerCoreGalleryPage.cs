using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class PickerCoreGalleryPage : CoreGalleryPage<Picker>
	{
		protected override bool SupportsTapGestureRecognizer => false;

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);
			var itemsContainer = new ViewContainer<Picker>(Test.Picker.Items, new Picker());
			itemsContainer.View.Items.Add("Item 1");
			itemsContainer.View.Items.Add("Item 2");
			itemsContainer.View.Items.Add("Item 3");

			var selectedIndexContainer = new ViewContainer<Picker>(Test.Picker.SelectedIndex, new Picker());
			selectedIndexContainer.View.Items.Add("Item 1");
			selectedIndexContainer.View.Items.Add("Item 2");
			selectedIndexContainer.View.Items.Add("Item 3");
			selectedIndexContainer.View.SelectedIndex = 2;

			var titleContainer = new ViewContainer<Picker>(Test.Picker.Title, new Picker());
			titleContainer.View.Title = "Title";

			var titleColorContainer = new ViewContainer<Picker>(Test.Picker.TitleColor, new Picker());
			titleColorContainer.View.Title = "Title Color";
			titleColorContainer.View.TitleColor = Color.Red;

			var buttonReset = new Button() { Text = "Reset color to default" };
			var buttonChange = new Button() { Text = "Change color" };
			buttonReset.Clicked += (o, a) => titleColorContainer.View.ClearValue(Picker.TitleColorProperty);
			buttonChange.Clicked += (o, a) => titleColorContainer.View.TitleColor = Color.Green;

			titleColorContainer.ContainerLayout.Children.Add(buttonReset);
			titleColorContainer.ContainerLayout.Children.Add(buttonChange);

			var textColorContainer = new ViewContainer<Picker>(Test.Picker.TextColor, new Picker());
			textColorContainer.View.Items.Add("Item 1");
			textColorContainer.View.Items.Add("Item 2");
			textColorContainer.View.Items.Add("Item 3");

			var fontAttributesContainer = new ViewContainer<Picker>(Test.Picker.FontAttributes,
				new Picker { FontAttributes = FontAttributes.Bold });
			fontAttributesContainer.View.Items.Add("Item 1");
			fontAttributesContainer.View.Items.Add("Item 2");
			fontAttributesContainer.View.Items.Add("Item 3");

			var fontFamilyContainer = new ViewContainer<Picker>(Test.Picker.FontFamily,
				new Picker());
			// Set font family based on available fonts per platform
			switch (Device.RuntimePlatform)
			{
				case Device.Android:
					fontFamilyContainer.View.FontFamily = "sans-serif-thin";
					break;
				case Device.iOS:
					fontFamilyContainer.View.FontFamily = "Courier";
					break;
				default:
					fontFamilyContainer.View.FontFamily = "Garamond";
					break;
			}
			fontFamilyContainer.View.Items.Add("Item 1");
			fontFamilyContainer.View.Items.Add("Item 2");
			fontFamilyContainer.View.Items.Add("Item 3");

			var fontSizeContainer = new ViewContainer<Picker>(Test.Picker.FontSize,
				new Picker { FontSize = 24 });
			fontSizeContainer.View.Items.Add("Item 1");
			fontSizeContainer.View.Items.Add("Item 2");
			fontSizeContainer.View.Items.Add("Item 3");

			var horizontalTextAlignmentContainer = new ViewContainer<Picker>(Test.Picker.HorizontalTextAlignment,
				new Picker { HorizontalTextAlignment = TextAlignment.End });

			horizontalTextAlignmentContainer.View.Items.Add("Item 1");
			horizontalTextAlignmentContainer.View.Items.Add("Item 2");
			horizontalTextAlignmentContainer.View.Items.Add("Item 3");

			var verticalTextAlignmentContainer = new ViewContainer<Picker>(Test.Picker.VerticalTextAlignment,
				new Picker { VerticalTextAlignment = TextAlignment.End });

			verticalTextAlignmentContainer.View.Items.Add("Item 1");
			verticalTextAlignmentContainer.View.Items.Add("Item 2");
			verticalTextAlignmentContainer.View.Items.Add("Item 3");

			Add(itemsContainer);
			Add(selectedIndexContainer);
			Add(titleContainer);
			Add(titleColorContainer);
			Add(textColorContainer);
			Add(fontAttributesContainer);
			Add(fontFamilyContainer);
			Add(fontSizeContainer);
			Add(horizontalTextAlignmentContainer);
			Add(verticalTextAlignmentContainer);
		}
	}
}