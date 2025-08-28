using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25598, "IndicatorView with Template won't show when ItemSource reaches 0 Elements", PlatformAffected.Android)]
	public partial class Issue25598 : ContentPage
	{
		public Issue25598()
		{
			InitializeComponent();
			BindingContext = new Issue25598ViewModel();
		}
	}

	public class Issue25598ViewModel : ViewModel
	{
		int _lastItemIndex = 3;

		private ObservableCollection<string> _items = new() { "Item1", "Item2", "Item3" };

		public ObservableCollection<string> Items
		{
			get => _items;
			set
			{
				if (_items != value)
				{
					_items = value;
					OnPropertyChanged(nameof(Items));
				}
			}
		}

		public Command AddRandomItemCommand => new(() => Items.Add($"Item{++_lastItemIndex}"));

		public Command RemoveCurrentItemCommand => new Command<int>(index =>
		{
			if (index >= 0 && index < Items.Count)
				Items.RemoveAt(index);
		});
	}
}