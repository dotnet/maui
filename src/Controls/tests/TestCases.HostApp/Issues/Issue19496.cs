namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 19496, "The Shell item text disappears when it is updated dynamically at runtime", PlatformAffected.UWP)]
	public partial class Issue19496 : TestShell
	{
		int count = 0;
		protected override void Init()
		{
			FlyoutBackgroundColor = Colors.Gray;
			FlyoutWidth = 85;
			FlyoutBehavior = FlyoutBehavior.Locked;

			Label label = new Label
			{
				Text = count.ToString(),
				AutomationId = "FlyoutItemLabel",
				TextColor = Colors.White
			};

			ContentPage contentPage = new ContentPage();
			contentPage.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

			contentPage.Content = new Button()
			{
				Text = "Update Label Text",
				AutomationId = "button",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Command = new Command(() =>
				{
					count++;
					label.Text = count.ToString();
				})
			};

			DataTemplate dataTemplate = new DataTemplate(() =>
			{

				return label;
			});

			this.ItemTemplate = dataTemplate;

			base.AddFlyoutItem(contentPage, "MainPage");
		}
	}
}