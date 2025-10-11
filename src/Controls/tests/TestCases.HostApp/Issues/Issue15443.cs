using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15443, "Programmatic position set should work as expected", PlatformAffected.UWP)]
	internal class Issue15443 : ContentPage
	{
		public Issue15443()
		{
			this.BindingContext = new Issue15443ViewModel();

			var positionLabel = new Label
			{
				AutomationId = "15443PositionLabel",
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.Black
			};
			positionLabel.SetBinding(Label.TextProperty, new Binding("CarouselCurrentItemIndex", BindingMode.OneWay, null, null, "Position: {0}"));


			// Create the CarouselView
			var carouselView = new CarouselView
			{
				WidthRequest = 200,
				HeightRequest = 200,
				Loop = false,
				Background = Colors.LightBlue
			};
			carouselView.SetBinding(CarouselView.ItemsSourceProperty, "Items");
			carouselView.SetBinding(CarouselView.PositionProperty, "CarouselCurrentItemIndex");

			// Carousel item template
			carouselView.ItemTemplate = new DataTemplate(() =>

					{

						var label = new Label
						{
							TextColor = Colors.Black,
							HorizontalTextAlignment = TextAlignment.Center,
							BackgroundColor = Colors.LightBlue
						};
						label.SetBinding(Label.TextProperty, "Title");

						var stack = new StackLayout
						{
							Orientation = StackOrientation.Vertical,
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center,
							WidthRequest = 200,

							Children = { label }
						};

						return new ContentView
						{
							Content = stack
						};
					});

			// Buttons for navigation
			var button1 = new Button
			{
				Text = "1",
				AutomationId = "15443Button1",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center
			};
			button1.SetBinding(Button.CommandProperty, "OneCommand");

			var button2 = new Button
			{
				Text = "2",
				AutomationId = "15443Button2",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center
			};
			button2.SetBinding(Button.CommandProperty, "TwoCommand");

			var button3 = new Button
			{
				Text = "3",
				AutomationId = "15443Button3",
				TextColor = Colors.Black,
				HorizontalOptions = LayoutOptions.Center
			};
			button3.SetBinding(Button.CommandProperty, "ThreeCommand");

			var buttonStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Spacing = 4,
				HorizontalOptions = LayoutOptions.Center,
				HeightRequest = 50,
				WidthRequest = 200,
				BackgroundColor = Colors.LightGray,
				Children = { button1, button2, button3 }
			};

			// Main stack layout
			var mainStack = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				AutomationId = "15443Stack",
				Spacing = 8,
				Padding = 8,
				Children = { carouselView, buttonStack, positionLabel }
			};

			// Set the Content of the Page
			Content = mainStack;
		}
	}
}
class Issue15443ViewModel : INotifyPropertyChanged
{
	private ObservableCollection<Issue15443DemoData> _items;

	public ObservableCollection<Issue15443DemoData> Items
	{
		get
		{
			return _items;
		}
		private set
		{
			_items = value;
			OnPropertyChanged(nameof(Items));
		}
	}

	private int _carouselCurrentItemIndex;
	public int CarouselCurrentItemIndex
	{
		get
		{
			return _carouselCurrentItemIndex;
		}
		private set
		{
			_carouselCurrentItemIndex = value;
			OnPropertyChanged(nameof(CarouselCurrentItemIndex));
		}
	}
	public Command OneCommand { get; }

	public Command TwoCommand { get; }

	public Command ThreeCommand { get; }

	public Issue15443ViewModel()
	{

		Items = new ObservableCollection<Issue15443DemoData>()
			{
				new Issue15443DemoData("One"),
				new Issue15443DemoData("Two"),
				new Issue15443DemoData("Three"),
			};

		CarouselCurrentItemIndex = 0;

		OneCommand = new Command(() =>
		{
			CarouselCurrentItemIndex = 0;

		});

		TwoCommand = new Command(() =>
		{
			CarouselCurrentItemIndex = 1;
		});

		ThreeCommand = new Command(() =>
		{
			CarouselCurrentItemIndex = 2;

		});

	}
	public event PropertyChangedEventHandler PropertyChanged;

	public void OnPropertyChanged(string name)
	{
		if (this.PropertyChanged != null)
			this.PropertyChanged(this, new PropertyChangedEventArgs(name));
	}

}

class Issue15443DemoData
{
	public string Title { get; set; }

	public Issue15443DemoData(string title)
	{
		Title = title;

	}
}
