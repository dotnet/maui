namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2681, "[UWP] Label inside Listview gets stuck inside infinite loop", PlatformAffected.UWP)]
public class Issue2681 : TestNavigationPage
{
	const string NavigateToPage = "Click Me.";
	protected override void Init()
	{
		PushAsync(new ContentPage() { Title = "Freeze Test", Content = new Button() { AutomationId = "NavigateToPage", Text = NavigateToPage, Command = new Command(() => this.PushAsync(new FreezeMe())) } });
	}


	public class FreezeMe : ContentPage
	{
		public List<int> Items { get; set; }

		public FreezeMe()
		{
			this.BindingContext = this;
			var lv = new ListView()
			{
				Margin = new Thickness(20, 5, 5, 5)
			};

			lv.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label() { Text = "sassifrass" };
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(AutomationIdProperty, new Binding(".", stringFormat: "{0}"));
				return new ViewCell() { View = label };
			});

			lv.SetBinding(ListView.ItemsSourceProperty, "Items");

			this.Content = new ScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label(){ Text = "If page is not frozen this test has passed" },
						new StackLayout()
						{
							Orientation = StackOrientation.Horizontal,
							Children = {lv  }
						}
					}
				}
			};

			this.Appearing += (s, e) =>
			{
				this.Items = new List<int> { 1, 2, 3 };
				this.OnPropertyChanged("Items");
			};
		}
	}
}
