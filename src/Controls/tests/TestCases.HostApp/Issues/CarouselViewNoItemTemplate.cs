using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 12777, "[Bug] CarouselView NRE if item template is not specified", PlatformAffected.iOS)]
	public class CarouselViewNoItemTemplate : ContentPage
	{
		public CarouselViewNoItemTemplate()
		{
			BindingContext = new Issue12777ViewModel();

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = new Thickness(12),
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Without exceptions, the test has passed.",
				AutomationId = "TestCarouselView"
			};

			var carouselView = new CarouselView();

			carouselView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue12777ViewModel.Items));

			layout.Children.Add(instructions);
			layout.Children.Add(carouselView);

			Content = layout;
		}
	}

	public class Issue12777Model
	{
		public Color Color { get; set; }
		public string Name { get; set; }
	}


	public class Issue12777ViewModel : BindableObject
	{
		ObservableCollection<Issue12777Model> _items;

		public Issue12777ViewModel()
		{
			LoadItems();
		}

		public ObservableCollection<Issue12777Model> Items
		{
			get { return _items; }
			set
			{
				_items = value;
				OnPropertyChanged();
			}
		}

		void LoadItems()
		{
			Items = new ObservableCollection<Issue12777Model>();

			var random = new Random();

			for (int n = 0; n < 5; n++)
			{
				Items.Add(new Issue12777Model
				{
					Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					Name = $"{n + 1}"
				});
			}
		}
	}
}