using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	internal class Issue17400Item : BindableObject
	{
		public string Label { get; set; }

		public string Description { get; set; }
	}

	internal class Issue17400ViewModel : BindableObject
	{
		ObservableCollection<Issue17400Item> _items;
		bool _areItemsVisible;

		public Issue17400ViewModel()
		{
			List<Issue17400Item> items = new List<Issue17400Item>();

			foreach (var item in Enumerable.Range(0, 20))
			{
				items.Add(new Issue17400Item
				{
					Label = "Label " + item,
					Description = "Description " + item,
				});
			}

			Items = new ObservableCollection<Issue17400Item>(items);
			AreItemsVisible = false;
		}

		public ObservableCollection<Issue17400Item> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public bool AreItemsVisible
		{
			get { return _areItemsVisible; }
			set
			{
				_areItemsVisible = value;
				OnPropertyChanged();
			}
		}

		public ICommand UpdateCommand => new Command(Update);

		void Update()
		{
			AreItemsVisible = !AreItemsVisible;
		}
	}

	[Issue(IssueTracker.Github, 17400, "CollectionView wrong Layout", PlatformAffected.UWP)]
	public partial class Issue17400 : ContentPage
	{
		public Issue17400()
		{
			InitializeComponent();

			BindingContext = new Issue17400ViewModel();
		}
	}
}