namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2740, "System.NotSupportedException: Unable to activate instance of type Microsoft.Maui.Controls.Platform.Android.PageContainer from native handle", PlatformAffected.Android)]
public class Issue2740 : TestFlyoutPage
{
	protected override void Init()
	{
		var page = new AddressListView();

		// Initialize ui here instead of ctor
		Flyout = new ContentPage
		{
			Content = new Label
			{
				Text = "Click a item on the left then the toolbar item switch"
			},
			Title = "2740"
		};
		Detail = new NavigationPage(page);
	}

	public class AddressListView : ContentPage
	{

		public AddressListView()
		{
			var listview = new ListView();
			listview.ItemsSource = new List<string> { "1", "2" };

			listview.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, new Binding("."));
				var viewCell = new ViewCell
				{
					View = label
				};

				return viewCell;
			});

			listview.ItemTapped += OnItemTapped;
			Content = listview;
			Title = "Unit List";
		}

		public async void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			var p = new UnitViolationView();
			await Navigation.PushAsync(p);
		}
	}

	public class UnitViolationView : ContentPage
	{
		public UnitViolationView()
		{
			ToolbarItems.Add(new ToolbarItem("Switch", null, MapAddressSwitch) { AutomationId = "Switch" });
		}

		async void MapAddressSwitch()
		{
			await Navigation.PopAsync(false);
			(Application.Current.MainPage as FlyoutPage).Detail = new NavigationPage(new AddressListView());
		}
	}
}