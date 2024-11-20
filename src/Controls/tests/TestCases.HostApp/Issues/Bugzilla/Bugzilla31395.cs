namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 31395, "Crash when switching MainPage and using a Custom Render")]
	public class Bugzilla31395 : NavigationPage
	{
		public Bugzilla31395() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Content = new CustomContentView
				{ // Replace with ContentView and everything works fine
					Content = new StackLayout
					{
						VerticalOptions = LayoutOptions.Center,
						Children = {
						new Button {
							AutomationId = "SwitchMainPage",
							Text = "Switch Main Page",
							Command = new Command (() => SwitchMainPage ())
						}
					}
					}
				};
			}

			void SwitchMainPage()
			{
				Application.Current.MainPage = new ContentPage { Content = new Label { Text = "Hello" } };
			}

			public class CustomContentView : ContentView
			{

			}
		}
	}
}