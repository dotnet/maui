using System;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 7678, "[iOS] CarouselView binded to a ObservableCollection and add Items later, crash", PlatformAffected.iOS)]
public class Issue7678 : TestContentPage
{
	public Issue7678()
	{
		Title = "Issue 7678";
		BindingContext = new Issue7678ViewModel();
	}
	protected override void Init()
	{
		var layout = new StackLayout();

		var instructions = new Label
		{
			Margin = new Thickness(6),
			Text = "The CarouselView below should render 5 items."
		};

		var itemsLayout =
			new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				SnapPointsType = SnapPointsType.MandatorySingle,
				SnapPointsAlignment = SnapPointsAlignment.Center
			};

		var carouselView = new CarouselView
		{
			AutomationId = "carouselView",
			ItemsLayout = itemsLayout,
			ItemTemplate = GetCarouselTemplate()
		};

		carouselView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		layout.Children.Add(instructions);
		layout.Children.Add(carouselView);

		Content = layout;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await ((Issue7678ViewModel)BindingContext).LoadItemsAsync();
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

			var border = new Border
			{
				Content = grid,
			};

			border.SetBinding(BackgroundColorProperty, new Binding("Color"));

			return border;
		});
	}

	public class Issue7678Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}

	public class Issue7678ViewModel : BindableObject
	{
		ObservableCollection<Issue7678Model> _items;

		public ObservableCollection<Issue7678Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		public async Task LoadItemsAsync()
		{
			Items = new ObservableCollection<Issue7678Model>();

			await Task.Delay(500);

			var random = new Random();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new Issue7678Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}
		}

	}
}
