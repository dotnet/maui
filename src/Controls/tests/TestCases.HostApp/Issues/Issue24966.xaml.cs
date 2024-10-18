using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.Specialized;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 24966, "CollectionView, the footer moves to the bottom of the page when the empty view or empty view template is enabled")]
	public partial class Issue24966
	{
		public Issue24966()
		{
			InitializeComponent();
			BindingContext = new Issue24966ViewModel();
		}
	}
	internal class Issue24966ViewModel
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

		public Issue24966ViewModel()
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
