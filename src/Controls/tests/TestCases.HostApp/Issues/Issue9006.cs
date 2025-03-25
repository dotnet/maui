namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 9006, "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar",
		PlatformAffected.iOS)]
	public class Issue9006 : TestShell
	{
		protected override void Init()
		{
			Routing.RegisterRoute("Issue9006_ContentPage", typeof(IntermediatePage));
			Routing.RegisterRoute("Issue9006_FinalPage", typeof(ContentPage));

			var contentPage = AddBottomTab("Tab 1");
			Items[0].CurrentItem.AutomationId = "Tab1AutomationId";
			AddBottomTab("Ignore Me");

			Label label = new Label()
			{
				Text = "Clicking on the first tab should pop you back to the root",
				AutomationId = "FinalLabel"
			};

			Button button = null;
			bool navigated = false;
			button = new Button()
			{
				Text = "Click Me",
				AutomationId = "ClickMe",
				Command = new Command(async () =>
				{
					await GoToAsync("Issue9006_ContentPage");
					await GoToAsync("Issue9006_FinalPage");

					button.Text = "Click me again. If pages get pushed again then test has passed.";
					DisplayedPage.Content = new StackLayout()
					{
						Children =
						{
							label
						}
					};
					if (navigated)
						label.Text = "Success";

					navigated = true;
				})
			};

			contentPage.Content = new StackLayout()
			{
				Children =
				{
					button
				}
			};
		}

		public class IntermediatePage : ContentPage
		{
			public IntermediatePage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "This is the intermediate page"
						}
					}
				};
			}
		}
	}
}
