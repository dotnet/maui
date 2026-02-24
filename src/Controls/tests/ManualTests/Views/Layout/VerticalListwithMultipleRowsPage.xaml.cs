namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListwithMultipleRowsPage : ContentPage
	{
		public VerticalListwithMultipleRowsPage()
		{
			InitializeComponent();

			myCollection.ItemsSource = new List<string> { "first", "second", "third", "first", "second", "third", "first", "second", "third", "first", "second", "third", "first", "second", "third", "first", "second", "third", "first", "second", "third" };
		}
	}
}

