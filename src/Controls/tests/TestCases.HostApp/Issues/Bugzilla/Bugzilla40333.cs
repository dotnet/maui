namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 40333, "[Android] IllegalStateException: Recursive entry to executePendingTransactions", PlatformAffected.Android)]
public class Bugzilla40333 : TestNavigationPage
{
	const string StartNavPageTestId = "StartNavPageTest";
	const string OpenRootId = "OpenRoot";
	const string StartTabPageTestId = "StartTabPageTest";
	const string StillHereId = "3 Still Here";
	const string ClickThisId = "2 Click This";

	protected override void Init()
	{
		var navButton = new Button { Text = "Test With NavigationPage", AutomationId = StartNavPageTestId };
		navButton.Clicked += (sender, args) => { PushAsync(new _40333MDP(false)); };

		var tabButton = new Button { Text = "Test With TabbedPage", AutomationId = StartTabPageTestId };
		tabButton.Clicked += (sender, args) => { PushAsync(new _40333MDP(true)); };

		var content = new ContentPage
		{
			Content = new StackLayout
			{
				Children = { navButton, tabButton }
			}
		};

		PushAsync(content);
	}


	public class _40333MDP : TestFlyoutPage
	{
		readonly bool _showTabVersion;

		public _40333MDP(bool showTabVersion)
		{
			_showTabVersion = showTabVersion;
		}

		protected override void Init()
		{
			if (_showTabVersion)
			{
				Flyout = new NavigationPage(new _40333TabPusher("Root")) { Title = "RootNav" };
				Detail = new TabbedPage() { Title = "DetailNav", Children = { new _40333DetailPage("T1") } };
			}
			else
			{
				Flyout = new NavigationPage(new _40333NavPusher("Root")) { Title = "RootNav" };
				Detail = new NavigationPage(new _40333DetailPage("Detail") { Title = "DetailPage" }) { Title = "DetailNav" };
			}
		}


		public class _40333DetailPage : ContentPage
		{
			public _40333DetailPage(string title)
			{
				Title = title;

				var openRoot = new Button
				{
					Text = "Open Flyout",
					AutomationId = OpenRootId
				};

				openRoot.Clicked += (sender, args) => ((FlyoutPage)Parent.Parent).IsPresented = true;

				Content = new StackLayout()
				{
					Children = { new Label { Text = "Detail Text" }, openRoot }
				};
			}
		}


		public class _40333NavPusher : ContentPage
		{
			readonly ListView _listView = new ListView();

			public _40333NavPusher(string title)
			{
				Title = title;

				_listView.ItemTemplate = new DataTemplate(() =>
				{
					var lbl = new Label();
					lbl.SetBinding(Label.TextProperty, ".");
					lbl.SetBinding(Label.AutomationIdProperty, ".");
					lbl.AutomationId = lbl.Text;

					var result = new ViewCell
					{
						View = new StackLayout
						{
							Orientation = StackOrientation.Horizontal,
							Children =
							{
								lbl
							}
						}
					};

					return result;
				});

				_listView.ItemsSource = new[] { "1", ClickThisId, StillHereId };
				_listView.ItemTapped += OnItemTapped;

				Content = new StackLayout
				{
					Children = { _listView }
				};
			}

			async void OnItemTapped(object sender, EventArgs e)
			{
				var rootNav = ((FlyoutPage)this.Parent.Parent).Flyout.Navigation;

				var newTitle = $"{Title}.{_listView.SelectedItem}";
				await rootNav.PushAsync(new _40333NavPusher(newTitle));
			}

			protected override async void OnAppearing()
			{
				base.OnAppearing();

				var newPage = new _40333DetailPage(Title);

				var detailNav = ((FlyoutPage)this.Parent.Parent).Detail.Navigation;
				var currentRoot = detailNav.NavigationStack[0];
				detailNav.InsertPageBefore(newPage, currentRoot);
				await detailNav.PopToRootAsync();
			}
		}


		public class _40333TabPusher : ContentPage
		{
			readonly ListView _listView = new ListView();

			public _40333TabPusher(string title)
			{
				Title = title;

				_listView.ItemTemplate = new DataTemplate(() =>
				{
					var lbl = new Label();
					lbl.SetBinding(Label.TextProperty, ".");
					lbl.SetBinding(Label.AutomationIdProperty, ".");
					lbl.AutomationId = lbl.Text;

					var result = new ViewCell
					{
						View = new StackLayout
						{
							Orientation = StackOrientation.Horizontal,
							Children =
							{
								lbl
							}
						}
					};

					return result;
				});

				_listView.ItemsSource = new[] { "1", ClickThisId, StillHereId };
				_listView.ItemTapped += OnItemTapped;

				Content = new StackLayout
				{
					Children = { _listView }
				};
			}

			async void OnItemTapped(object sender, EventArgs e)
			{
				var rootNav = ((FlyoutPage)this.Parent.Parent).Flyout.Navigation;

				var newTitle = $"{Title}.{_listView.SelectedItem}";
				await rootNav.PushAsync(new _40333TabPusher(newTitle));
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();

				var newPage = new _40333DetailPage(Title);

				var detailTab = (TabbedPage)((FlyoutPage)this.Parent.Parent).Detail;

				detailTab.Children.Add(newPage);
				detailTab.CurrentPage = newPage;
			}
		}
	}
}