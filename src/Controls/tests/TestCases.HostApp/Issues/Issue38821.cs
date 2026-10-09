using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38821, "CarouselView resets CurrentItem after removing the current item", PlatformAffected.Android)]
public class Issue38821 : ContentPage
{
	readonly Issue38821ViewModel _viewModel = new();

	public Issue38821()
	{
		var carousel = new CarouselView
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
				label.SetBinding(
		Label.AutomationIdProperty,
		new Binding(".", stringFormat: "CarouselItem{0}"));

				return label;
			})
		};

		carousel.SetBinding(
		 ItemsView.ItemsSourceProperty,
		 nameof(Issue38821ViewModel.Items));

		carousel.SetBinding(
		 CarouselView.CurrentItemProperty,
		 nameof(Issue38821ViewModel.CurrentItem));

		var currentItem = new Label
		{
			AutomationId = "CurrentValue"
		};

		currentItem.SetBinding(
		 Label.TextProperty,
		 new Binding(
		  nameof(Issue38821ViewModel.CurrentItem),
		  stringFormat: "Current: {0}"));

		var removeButton = new Button
		{
			AutomationId = "RemoveCurrentItem",
			Text = "Remove current item and select 2"
		};

		removeButton.Clicked += (_, _) =>
		{
			var currentItem = carousel.CurrentItem.ToString();
			if (currentItem is not null)
				_viewModel.Items?.Remove(currentItem);
			_viewModel.CurrentItem = "2";
		};

		var grid = new Grid
		{
			RowDefinitions =
   {
	new RowDefinition(GridLength.Star),
	new RowDefinition(GridLength.Auto),
	new RowDefinition(GridLength.Auto)
   }
		};

		grid.Add(carousel);
		grid.Add(currentItem, 0, 1);
		grid.Add(removeButton, 0, 2);

		BindingContext = _viewModel;
		Content = grid;
	}

	sealed class Issue38821ViewModel : INotifyPropertyChanged
	{
		string _currentItem = "1";

		public ObservableCollection<string> Items { get; } =
		 new() { "0", "1", "2", "3", "4", "5" };

		public string CurrentItem
		{
			get => _currentItem;
			set
			{
				if (_currentItem == value)
					return;

				_currentItem = value;
				PropertyChanged?.Invoke(
				 this,
				 new PropertyChangedEventArgs(nameof(CurrentItem)));
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
