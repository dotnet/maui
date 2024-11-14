namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24547, "[Windows] FlyoutPage ShouldShowToolbarButton when overridden to return false, still shows button in title bar", PlatformAffected.UWP)]
	public class Issue24547 : NavigationPage
	{
	
		public Issue24547() : base(new Issue24547PopoverPage())
		{

		}
		public class Issue24547PopoverPage : FlyoutPage
		{
			public Issue24547PopoverPage()
			{
				FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

				Flyout = new ContentPage
				{
					Title = "Flyout",
					BackgroundColor = Colors.Red,
					Content = new StackLayout
					{
						Children = {
							new Label { Text = "Flyout" }
						}
					}
				};

				ContentPage contentPage = new ContentPage
				{
					BackgroundColor = Colors.Green,
					Title = "Detail",
					Content = new StackLayout
					{
						Children = 
						{
							CreateDetailButton()
						}
					}
				};

				Detail = new NavigationPage(contentPage);
				Button button = new Button() { Text = "Menu", AutomationId = "MenuButton" };
				button.Clicked += (s, e) => IsPresented = true;
				NavigationPage.SetTitleView(contentPage, button);
			}

			private Button CreateDetailButton()
			{
				Button button = new Button
				{
					Text = "Detail",
					AutomationId = "DetailButton"
				};

				button.Clicked += OnDetailButtonClicked;
				return button;
			}

			private async void OnDetailButtonClicked(object sender, EventArgs e)
			{
				await this.Window.Page.Navigation.PushAsync(new NavigationPage(new Issue24547Page1()));
			}

			public override bool ShouldShowToolbarButton()
			{
				return false;
			}
		}

		public class Issue24547Page1 : ContentPage
		{
			public Issue24547Page1()
			{
				Title = "Content Page";
				VerticalStackLayout layout = new VerticalStackLayout
				{
					Children =
					{
						CreatePopButton()
					}
				};
				Content = layout;

				ToolbarItems.Add(new ToolbarItem
				{
					Text = "Item One",
				});
			}

			private Button CreatePopButton()
			{
				Button button = new Button
				{
					Text = "Pop Button",
					AutomationId = "PopButton",
				};

				button.Clicked += OnPopButtonClicked;
				return button;
			}

			private async void OnPopButtonClicked(object sender, EventArgs e)
			{
				await this.Window.Page.Navigation.PopAsync();
			}
		}
	}
}