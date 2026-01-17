using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20274, "IsEnabled=False on CollectionView not working", PlatformAffected.All)]
	public partial class Issue20274 : ContentPage
	{
		public ObservableCollection<CollectionViewItem> Items { get; set; }

		public Issue20274()
		{
			Items = new ObservableCollection<CollectionViewItem>
			{
				new CollectionViewItem { Name = "Item 1" },
				new CollectionViewItem { Name = "Item 2" },
				new CollectionViewItem { Name = "Item 3" }
			};
			InitializeComponent();
			BindingContext = this;
			label.Text = Items[0].Name;
		}

		void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection?.FirstOrDefault() is CollectionViewItem selectedItem)
			{
				selectedItem.Name = $"{selectedItem.Name} - Selected";
				this.label.Text = selectedItem.Name;
			}
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			collectionView.IsEnabled = !collectionView.IsEnabled;
		}
	}

	public class CollectionViewItem : INotifyPropertyChanged
	{
		string _name;
		public string Name
		{
			get => _name;
			set
			{
				if (_name != value)
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}