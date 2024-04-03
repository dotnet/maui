using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21374, "Error when adding to ObservableCollection", PlatformAffected.iOS)]
	public partial class Issue21374 : ContentPage
	{
		public Issue21374()
		{
			InitializeComponent();
		}
	}

	public class Issue21374Model
	{
		public string Text { get; set; }
	}

	public class Issue21374ViewModel : INotifyPropertyChanged
	{
		string _success;
		ObservableCollection<Issue21374Model> _items = new ObservableCollection<Issue21374Model>();

		public ICommand PopulateItemsCommand => new Command(PopulateItems);

		public string Success
		{
			get => _success;
			set
			{
				_success = value;
				OnPropertyChanged("Success");
			}
		}

		public ObservableCollection<Issue21374Model> Items
		{
			get => _items;
			set
			{
				if (_items != value)
				{
					_items = value;
					OnPropertyChanged("Items");
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		void PopulateItems()
		{
			try
			{
				for (int j = 0; j < 10; j++)
				{
					Items.Add(new Issue21374Model { Text = $"Item {j + 1}" });
				}

				Success = "Success";
			}
			catch
			{
				Success = "Failed";
			}
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChangedEventHandler changed = PropertyChanged;

			if (changed == null)
			{
				return;
			}

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}