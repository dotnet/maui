using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Microsoft.Maui.ManualTests.Views
{
	public partial class VerticalListSwitchVisibilityPage : ContentPage
	{
		public VerticalListSwitchVisibilityPage()
		{
			InitializeComponent();
			BindingContext = new InitiallyHiddenItemsViewModel();
		}
	}

	public class MainItem
	{
		public string Label { get; set; }
		public string Description { get; set; }
	}

	public partial class InitiallyHiddenItemsViewModel : ObservableObject
	{
		[ObservableProperty]
		private ObservableCollection<MainItem> _items;

		[ObservableProperty]
		private bool _areItemsVisible;

		public InitiallyHiddenItemsViewModel()
		{
			List<MainItem> items = new List<MainItem>();

			foreach (var item in Enumerable.Range(0, 20))
			{
				items.Add(new MainItem
				{
					Label = "Label " + item,
					Description = "Description " + item,
				});
			}

			Items = new ObservableCollection<MainItem>(items);
		}

		[RelayCommand]
		private void Move()
		{
			AreItemsVisible = !AreItemsVisible;
		}
	}
}

