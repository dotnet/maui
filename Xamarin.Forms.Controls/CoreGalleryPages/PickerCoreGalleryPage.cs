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
			switch(Device.RuntimePlatform)
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

			Add(itemsContainer);
			Add(selectedIndexContainer);
			Add(titleContainer);
			Add(textColorContainer);
			Add(fontAttributesContainer);
			Add(fontFamilyContainer);
			Add(fontSizeContainer);
		}
	}
}