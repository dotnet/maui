using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 12777, "[Bug] CarouselView NRE if item template is not specified", PlatformAffected.iOS)]
public class Issue12777 : TestContentPage
{
	public Issue12777()
	{
		BindingContext = new Issue12777ViewModel1();
	}

	protected override void Init()
	{
		var layout = new StackLayout();

		var instructions = new Label
		{
			Padding = new Thickness(12),
			BackgroundColor = Colors.Black,
			TextColor = Colors.White,
			Text = "Without exceptions, the test has passed."
		};

		var carouselView = new CarouselView
		{
			AutomationId = "TestCarouselView"
		};

		carouselView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue12777ViewModel1.Items));

		layout.Children.Add(instructions);
		layout.Children.Add(carouselView);

		Content = layout;
	}
}


public class Issue12777Model1
{
	public Color Color { get; set; }
	public string Name { get; set; }
}


public class Issue12777ViewModel1 : BindableObject
{
	ObservableCollection<Issue12777Model1> _items;

	public Issue12777ViewModel1()
	{
		LoadItems();
	}

	public ObservableCollection<Issue12777Model1> Items
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
		Items = new ObservableCollection<Issue12777Model1>();

		var random = new Random();

		for (int n = 0; n < 5; n++)
		{
			Items.Add(new Issue12777Model1
			{
				Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
				Name = $"{n + 1}"
			});
		}
	}
}