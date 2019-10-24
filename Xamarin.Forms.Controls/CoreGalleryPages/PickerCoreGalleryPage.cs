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

			Add(itemsContainer);
			Add(selectedIndexContainer);
			Add(titleContainer);
			Add(textColorContainer);
		}
	}
}