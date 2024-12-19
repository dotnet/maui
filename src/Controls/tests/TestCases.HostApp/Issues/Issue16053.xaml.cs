using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16053, "ListView SelectedItem retains its value after ListView is cleared", PlatformAffected.All)]
	public partial class Issue16053 : ContentPage
	{
		public Issue16053()
		{
			InitializeComponent();
			BindingContext = new _16053ViewModel();
		}

		private void Button_Click(object sender, EventArgs e)
		{
			if (ListView1.SelectedItem is string selectedItem)
			{
				lbl.Text = $"Selected Item: {selectedItem}";
			}
			else
			{
				lbl.Text = "No item selected";
			}
		}

		private void Button_Click_1(object sender, EventArgs e)
		{
			if (BindingContext is _16053ViewModel viewModel)
			{
				viewModel.Items.Clear();
			}
		}
	}

	public class _16053ViewModel : INotifyPropertyChanged
	{
		private string _selectedItem;

		public ObservableCollection<string> Items { get; set; }

		public string SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (_selectedItem != value)
				{
					_selectedItem = value;
					OnPropertyChanged(nameof(SelectedItem));
				}
			}
		}

		public _16053ViewModel()
		{
			Items = new ObservableCollection<string>
			{
				"Coffee",
				"Tea",
				"Orange Juice",
				"Milk",
				"Iced Tea",
				"Mango Shake"
			};
		}
		public event PropertyChangedEventHandler PropertyChanged;


		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

}