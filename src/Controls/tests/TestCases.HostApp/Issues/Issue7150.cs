using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 7150, "EmptyView using Template displayed at the same time as the content", PlatformAffected.UWP)]
public class Issue7150 : TestContentPage
{
	public Issue7150()
	{
		Title = "Issue 7150";
		BindingContext = new Issue7150ViewModel();
	}
	protected override void Init()
	{
		var layout = new StackLayout();
		var filterButton = new Button
		{
			Margin = new Thickness(20),
			AutomationId = "FilterButton",
			Text = "Filter"
		};
		filterButton.SetBinding(Button.CommandProperty, "FilterCommand");

		var emptyViewContent = new StackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Children =
				{
					new Label
					{
						Text = "No results matched your filter.",
						Margin = new Thickness(10, 25, 10, 10),
						FontAttributes = FontAttributes.Bold,
						FontSize = 18,
						HorizontalTextAlignment = TextAlignment.Center
					},
					new Label
					{
						Text = "Try a broader filter?",
						FontAttributes = FontAttributes.Italic,
						FontSize = 12,
						HorizontalTextAlignment = TextAlignment.Center
					}
				}
		};

		var emptyView = new ContentView { Content = emptyViewContent };
		var carouselView = new CarouselView
		{
			ItemTemplate = GetCarouselTemplate(),
			EmptyView = emptyView,
		};

		carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");
		layout.Children.Add(filterButton);
		layout.Children.Add(carouselView);
		Content = layout;
	}

	internal DataTemplate GetCarouselTemplate()
	{
		return new DataTemplate(() =>
		{
			var grid = new Grid();
			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(6)
			};

			info.SetBinding(Label.TextProperty, new Binding("Name"));
			grid.Children.Add(info);
			return grid;
		});
	}

	public class Issue7150Model
	{
		public string Name { get; set; }
	}

	public class Issue7150ViewModel : BindableObject
	{
		ObservableCollection<Issue7150Model> _items;
		public ICommand FilterCommand => new Command(FilterItems);
		readonly IList<Issue7150Model> source;

		public Issue7150ViewModel()
		{
			source = new List<Issue7150Model>();
			source.Add(new Issue7150Model
			{
				Name = "Baboon"
			});
			source.Add(new Issue7150Model
			{
				Name = "Capuchin Monkey"
			});
			source.Add(new Issue7150Model
			{
				Name = "Blue Monkey"
			});

			Items = new ObservableCollection<Issue7150Model>(source);
		}

		public ObservableCollection<Issue7150Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public void FilterItems()
		{
			var filter = "Mandrill";
			var filteredItems = source.Where(monkey => monkey.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
			foreach (var monkey in source)
			{
				if (!filteredItems.Contains(monkey))
				{
					Items?.Remove(monkey);
				}
				else
				{
					if (Items != null && !Items.Contains(monkey))
					{
						Items.Add(monkey);
					}
				}
			}
		}
	}
}