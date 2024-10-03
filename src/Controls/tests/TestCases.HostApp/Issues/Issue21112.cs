namespace Controls.TestCases.HostApp.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21112, "TableView TextCell command executes only once", PlatformAffected.UWP)]
	public class Issue21112 : ContentPage
	{
		public Issue21112()
		{
			Title = "AbsoluteLayout demos";

			var tableView = new TableView
			{
				Intent = TableIntent.Menu,
				Root = new TableRoot
				{
					new TableSection("XAML")
					{
						new TextCell
						{
							Text = "Stylish header demo",
							AutomationId = "TextCell",
							Detail = "Absolute positioning and sizing",
							Command = new Command<Type>(async (pageType) =>
							{
								var page = (Page)Activator.CreateInstance(pageType);
								await Navigation.PushAsync(page);
							}),
							CommandParameter = typeof(StylishHeaderDemoPage)
						}
					}
				}
			};

			Content = tableView;
		}
	}

	public class StylishHeaderDemoPage : ContentPage
	{
		public StylishHeaderDemoPage()
		{
			Title = "Stylish header demo";

			var stackLayout = new StackLayout
			{
				Margin = new Thickness(20)
			};

			var button = new Button
			{
				WidthRequest = 150,
				AutomationId = "Button",
				Text = "click"
			};

			button.Clicked += Button_Clicked;

			stackLayout.Children.Add(button);

			Content = stackLayout;
		}

		private async void Button_Clicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}

	}
}
