using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16053, "ListView SelectedItem retains its value after ListView is cleared", PlatformAffected.All)]
	public partial class Issue16053 : ContentPage
	{
		ObservableCollection<string> Items;
		public Issue16053()
		{
			InitializeComponent();
			Items = new ObservableCollection<string>
			{
				"Coffee",
				"Tea",
				"Orange Juice",
				"Milk",
				"Iced Tea",
				"Mango Shake"
			};
			ListView1.ItemsSource = Items;
			ListView1.SelectedItem = Items[1];
		}

		private void Button_Click_1(object sender, EventArgs e)
		{
			Items.Clear();
		}

		private void Button_ChangeItems(object sender, EventArgs e)
		{
			Items = new ObservableCollection<string>
			{
				"Water",
				"Soda",
				"Lemon Juice",
				"Milk",
				"Hot Chocolate"
			};
			ListView1.ItemsSource = Items;
			ListView1.SelectedItem = Items[1];
		}

		private void Button_RemoveSelectedItem(object sender, EventArgs e)
		{
			Items.Remove(ListView1.SelectedItem.ToString());
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			if(ListView1.SelectedItem is not null)
			{
				lbl.Text = ListView1.SelectedItem.ToString();
			}
			else
			{
				lbl.Text = "null";
			}
		}
	}
}