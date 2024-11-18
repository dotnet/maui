using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8964, "Adding an item to the beginning of the bound ItemSource causes the carousel to skip sometimes", PlatformAffected.Android)]
public class Issue8964 : TestContentPage
{
	object _currentItem;
	int _counter;
	Label _lbl;

	protected override void Init()
	{
		ItemSourceUnderTest = new ObservableCollection<ModelIssue8964>(GetCarouselItems());
		var lbl = new Label
		{
			Text = "Scroll to the previous item until see the Item with counter 6, since we are inserting items on the start of the collection the position should be  the same"
		};
		CarouselViewUnderTest = new CarouselView
		{
			HeightRequest = 250,
			IsSwipeEnabled = true,
			ItemsSource = ItemSourceUnderTest,
			ItemTemplate = GetCarouselTemplate(),
			CurrentItem = _currentItem,
			AutomationId = "carouseView",
			Loop = false
		};
		CarouselViewUnderTest.CurrentItemChanged += CarouselViewUnderTestCurrentItemChanged;
		CarouselViewUnderTest.PositionChanged += CarouselViewUnderTest_PositionChanged;

		_lbl = new Label
		{
			Text = $"Item Position - {CarouselViewUnderTest.Position}"
		};

		var stackLayout = new StackLayout
		{
			lbl, CarouselViewUnderTest, _lbl,
		};

		stackLayout.VerticalOptions = LayoutOptions.Center;

		Content = stackLayout;
	}

	public ObservableCollection<ModelIssue8964> ItemSourceUnderTest { get; set; }
	public CarouselView CarouselViewUnderTest { get; set; }

	void CarouselViewUnderTest_PositionChanged(object sender, PositionChangedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"PositionChanged {CarouselViewUnderTest.Position}");
		_lbl.Text = $"Item Position - {e.CurrentPosition}";
	}

	void CarouselViewUnderTestCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
	{
		System.Diagnostics.Debug.WriteLine($"CurrentItemChanged {CarouselViewUnderTest.Position}");
		_counter++;
		ItemSourceUnderTest.Insert(0, new ModelIssue8964 { Name = $"Counter {_counter}", Color = Colors.Red, Index = _counter });
	}

	DataTemplate GetCarouselTemplate()
	{
		return new DataTemplate(() =>
		{

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Thickness(6)
			};

			info.SetBinding(Label.TextProperty, new Binding("Name"));

			var grid = new Grid()
			{
				info
			};

			var frame = new Frame
			{
				CornerRadius = 12,
				Content = grid,
				HasShadow = false,
				Margin = new Thickness(12)
			};

			frame.SetBinding(BackgroundColorProperty, new Binding("Color"));

			return frame;
		});
	}

	List<ModelIssue8964> GetCarouselItems()
	{
		var random = new Random();

		var items = new List<ModelIssue8964>();

		for (int n = 0; n < 5; n++)
		{
			items.Add(new ModelIssue8964
			{
				Color = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
				Name = DateTime.Now.AddDays(n).ToString("D"),
				Index = _counter
			});
			_counter++;
		}

		_currentItem = items[4];

		return items;
	}
}

public class ViewModelIssue8964
{
	public ViewModelIssue8964()
	{

	}
}

public class ModelIssue8964
{
	public Color Color { get; set; }
	public string Name { get; set; }
	public int Index { get; set; }

	public override string ToString()
	{
		return Name;
	}
}