using System.Collections.ObjectModel;
using System.Windows.Input;


namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11896, "CollectionView Header/Footer/EmptyView issues when adding/removing items", PlatformAffected.Android)]
	public partial class Issue11896
	{
		public Issue11896()
		{
			InitializeComponent();
			BindingContext = new Issue11896ViewModel();
		}
	}
	internal class Issue11896ViewModel
	{
		// Define a static list of cities to add to the collection
		private List<string> _staticCities = new()
		{
			"Paris",
			"New York",
			"Tokyo",
			"Berlin",
			"Madrid",
			"London"
		};

		private int _currentIndex = 0;

		public ObservableCollection<string> ItemList { get; set; }

		public ICommand AddCommand => new Command(Add);
		public ICommand RemoveCommand => new Command(Remove);

		public Issue11896ViewModel()
		{
			// Initialize the ItemList
			ItemList = new ObservableCollection<string>();
		}

		private void Add()
		{
			// Add the next city from the static list
			if (_currentIndex < _staticCities.Count)
			{
				ItemList.Add(_staticCities[_currentIndex]);
				_currentIndex++;
			}
			else
			{
				// Optionally reset the index or handle the end of the list as needed
				_currentIndex = 0;  // Resetting to allow cycling through the list again
			}
		}

		private void Remove()
		{
			// Remove the last item in the list if any exist
			if (ItemList.Count > 0)
			{
				ItemList.RemoveAt(ItemList.Count - 1);
				// Decrement the index to ensure the correct city is added next time
				_currentIndex = Math.Max(0, _currentIndex - 1);
			}
		}
	}
}
