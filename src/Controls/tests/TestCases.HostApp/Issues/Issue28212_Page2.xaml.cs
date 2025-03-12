using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	public partial class Issue28212_Page2 : ContentPage
	{
		public ObservableCollection<string> Items { get; } = [];
		public Issue28212_Page2()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			Items.Add("Item " + (Items.Count + 1));
		}
	}
}