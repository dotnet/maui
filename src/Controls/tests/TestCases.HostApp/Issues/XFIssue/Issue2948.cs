namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2948, "FlyoutPage Detail is interactive even when Flyout is open when in Landscape")]
public class Issue2948 : TestFlyoutPage
{
	static FlyoutPage s_mdp;

	protected override void Init()
	{
		s_mdp = this;
		var menuPage = new MenuPage();

		menuPage.Menu.ItemSelected += (sender, e) => NavigateTo(e.SelectedItem as MenuItem);

		Flyout = menuPage;
		Detail = new NavigationPage(new ContractsPage());
	}


	public class MenuListData : List<MenuItem>
	{
		public MenuListData()
		{
			Add(new MenuItem()
			{
				Title = "Contracts",
				IconSource = "bank.png",
				TargetType = typeof(ContractsPage)
			});

			Add(new MenuItem()
			{
				Title = "Leads",
				IconSource = "bank.png",
				TargetType = typeof(ContractsPage)
			});

			Add(new MenuItem()
			{
				Title = "Accounts",
				IconSource = "bank.png",
				TargetType = typeof(ContractsPage)
			});

			Add(new MenuItem()
			{
				Title = "Opportunities",
				IconSource = "bank.png",
				TargetType = typeof(ContractsPage)
			});
		}
	}


	public class ContractsPage : ContentPage
	{
		public ContractsPage()
		{
			Title = "Contracts";
			IconImageSource = "bank.png";

			var grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());

			var btn = new Button
			{
				HeightRequest = 300,
				HorizontalOptions = LayoutOptions.End,
				BackgroundColor = Colors.Pink,
				AutomationId = "btnOnDetail"
			};

			btn.Clicked += (object sender, EventArgs e) =>
			{
				DisplayAlertAsync("Clicked", "I was clicked", "Ok");
			};

			Grid.SetColumn(btn, 1);

			grid.Children.Add(btn);

			var showMasterButton = new Button
			{
				AutomationId = "ShowFlyoutBtn",
				Text = "Show Flyout"
			};
			showMasterButton.Clicked += (sender, e) =>
			{
				s_mdp.IsPresented = true;
			};

			Content = new ScrollView
			{

				Content = new StackLayout
				{
					Children = {
						showMasterButton,
						grid,
						new BoxView {
							HeightRequest = 100,
							Color = Colors.Red,
						},
						new BoxView {
							HeightRequest = 200,
							Color = Colors.Green,
						},
						new BoxView {
							HeightRequest = 300,
							Color = Colors.Red,
						},
						new BoxView {
							HeightRequest = 400,
							Color = Colors.Green,
						},
						new BoxView {
							HeightRequest = 500,
							Color = Colors.Red,
						}
					}
				},

			};
		}
	}


	public class MenuListView : ListView
	{
		public MenuListView()
		{
			List<MenuItem> data = new MenuListData();

			ItemsSource = data;
#pragma warning disable CS0618 // Type or member is obsolete
			VerticalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
			BackgroundColor = Colors.Transparent;

			var cell = new DataTemplate(typeof(ImageCell));
			cell.SetBinding(TextCell.TextProperty, "Title");
			cell.SetBinding(ImageCell.ImageSourceProperty, "IconSource");

			ItemTemplate = cell;
			SelectedItem = data[0];
		}
	}

	public class MenuPage : ContentPage
	{
		public ListView Menu { get; set; }

		public MenuPage()
		{
			Title = "Menu";
			BackgroundColor = Color.FromArgb("333333");

			Menu = new MenuListView();

			var menuLabel = new ContentView
			{
				Padding = new Thickness(10, 36, 0, 5),
				Content = new Label
				{
					TextColor = Color.FromArgb("AAAAAA"),
					Text = "MENU",
				}
			};

#pragma warning disable CS0618 // Type or member is obsolete
			var layout = new StackLayout
			{
				Spacing = 0,
				VerticalOptions = LayoutOptions.FillAndExpand
			};
#pragma warning restore CS0618 // Type or member is obsolete
			layout.Children.Add(menuLabel);
			layout.Children.Add(Menu);

			Content = layout;
		}
	}

	void NavigateTo(MenuItem menu)
	{
		var displayPage = (Page)Activator.CreateInstance(menu.TargetType);

		Detail = new NavigationPage(displayPage);

	}


	public class MenuItem
	{
		public string Title { get; set; }

		public string IconSource { get; set; }

		public Type TargetType { get; set; }
	}
}
