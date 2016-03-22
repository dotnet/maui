using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class PickerCoreGalleryPage : CoreGalleryPage<Picker>
	{
		// TODO
		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);
			var itemsContainer = new ViewContainer<Picker> (Test.Picker.Items, new Picker ());
			itemsContainer.View.Items.Add ("Item 1");
			itemsContainer.View.Items.Add ("Item 2");
			itemsContainer.View.Items.Add ("Item 3");

			var selectedIndexContainer = new ViewContainer<Picker> (Test.Picker.SelectedIndex, new Picker ());
			selectedIndexContainer.View.Items.Add ("Item 1");
			selectedIndexContainer.View.Items.Add ("Item 2");
			selectedIndexContainer.View.Items.Add ("Item 3");
			selectedIndexContainer.View.SelectedIndex = 2;

			var titleContainer = new ViewContainer<Picker> (Test.Picker.Title, new Picker ());
			titleContainer.View.Title = "Title";

			Add (itemsContainer);
			Add (selectedIndexContainer);
			Add (titleContainer);
		}
	}
}