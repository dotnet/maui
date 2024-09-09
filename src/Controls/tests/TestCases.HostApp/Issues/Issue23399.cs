namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23399, "Closing Modal While App is Backgrounded Fails", PlatformAffected.Android)]
	public class Issue23399: NavigationPage
	{
		
		public Issue23399() : base(new TestPage())
		{
		}
		
		public class TestPage : TestContentPage
		{
			protected override void Init()
			{
				Content = new VerticalStackLayout()
				{
					new Button()
					{
						Text = "Open Modal",
						AutomationId = "OpenModal",
						Command = new Command(async () =>
						{
							await Navigation.PushModalAsync(CreateDestinationPage());
						})
					}
				};
			}
			
			ContentPage CreateDestinationPage()
			{
				return new ContentPage()
				{
					Title = "Test",
					Content = new VerticalStackLayout()
					{
						new Button()
						{
							AutomationId = "StartCloseModal",
							Text = "Close Modal",
							Command = new Command(()=>
							{
								Navigation.PopModalAsync();
							})
						}
					}
				};
			}
		}
	}
}
