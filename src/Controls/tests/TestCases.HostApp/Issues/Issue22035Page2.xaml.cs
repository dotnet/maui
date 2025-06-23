using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	public partial class Issue22035Page2 : ContentPage
	{
		public Issue22035Page2()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ObservableCollection<Issue22035Model> Images { get; set; } = new();

		void OnLoadButtonPressed(object sender, EventArgs e)
		{
			Images.Clear();

			Images.Add(new Issue22035Model { Text = "Item 1", ImagePath = "photo21314.jpg" });
			Images.Add(new Issue22035Model { Text = "Item 2", ImagePath = "oasis.jpg" });
			Images.Add(new Issue22035Model { Text = "Item 3", ImagePath = "photo21314.jpg" });
			Images.Add(new Issue22035Model { Text = "Item 4", ImagePath = "oasis.jpg" });
		}
	}

	public class Issue22035Model
	{
		public string Text { get; set; }
		public string ImagePath { get; set; }
		public string AutomationId => Text.Replace(" ", "", StringComparison.InvariantCultureIgnoreCase);
	}
}