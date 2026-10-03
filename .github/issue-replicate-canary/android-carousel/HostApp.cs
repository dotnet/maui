#if ANDROID
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38821, "CarouselView resets CurrentItem after removing the current item", PlatformAffected.Android)]
public class Issue38821 : ContentPage
{
	public Issue38821()
	{
		var viewModel = new Issue38821ViewModel();
		var carouselView = new CarouselView
		{
			AutomationId = "Carousel",
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 40,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, new Binding(".", stringFormat: "CarouselItem{0}"));
				var itemRoot = new Grid();
				itemRoot.Add(label);
				return itemRoot;
			})
		};
		carouselView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue38821ViewModel.Items));
		carouselView.SetBinding(CarouselView.CurrentItemProperty, nameof(Issue38821ViewModel.CurrentItem), mode: BindingMode.TwoWay);
		var currentValue = new Label { AutomationId = "CurrentValue" };
		currentValue.SetBinding(Label.TextProperty, new Binding(nameof(Issue38821ViewModel.CurrentItem), stringFormat: "Current: {0}"));
		var itemsValue = new Label { AutomationId = "ItemsValue" };
		itemsValue.SetBinding(Label.TextProperty, nameof(Issue38821ViewModel.ItemsText));
		var removeButton = new Button
		{
			AutomationId = "RemoveCurrentItem",
			Text = "Remove current item and select 2"
		};
		removeButton.Clicked += (_, _) =>
		{
			if (viewModel.Items.Count >= 3 && viewModel.CurrentItem == "1")
			{
				viewModel.Items.Remove(viewModel.CurrentItem);
				viewModel.CurrentItem = "2";
			}
		};
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto)
			}
		};
		grid.Add(carouselView);
		Grid.SetRow(currentValue, 1);
		grid.Add(currentValue);
		Grid.SetRow(itemsValue, 2);
		grid.Add(itemsValue);
		Grid.SetRow(removeButton, 3);
		grid.Add(removeButton);
		BindingContext = viewModel;
		Content = grid;
	}

	public sealed class Issue38821ViewModel : INotifyPropertyChanged
	{
		string _currentItem = "1";

		public Issue38821ViewModel()
		{
			Items.CollectionChanged += (_, _) => OnPropertyChanged(nameof(ItemsText));
		}

		public ObservableCollection<string> Items { get; } = new() { "0", "1", "2" };

		public string CurrentItem
		{
			get => _currentItem;
			set
			{
				if (_currentItem == value)
					return;
				_currentItem = value;
				OnPropertyChanged();
			}
		}

		public string ItemsText => $"Items: {string.Join(",", Items)}";
		public event PropertyChangedEventHandler? PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
#endif
