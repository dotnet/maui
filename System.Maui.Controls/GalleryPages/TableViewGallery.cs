namespace Xamarin.Forms.Controls
{
	internal class TableViewGallery : ContentPage
	{
		public TableViewGallery () {

			var section = new TableSection ("Section One") {
				new ViewCell { View = new Label { Text = "View Cell 1" } },
				new ViewCell { View = new Label { Text = "View Cell 2" } }
			};

			var root = new TableRoot ("Table") {
				section
			};

			var tableLayout = new TableView {
				Root = root,
				RowHeight = 100
			};

			Content = tableLayout;
		}
	}
}
