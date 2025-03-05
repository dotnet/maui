namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Flyout Header Behavior",
	   PlatformAffected.All)]
public class ShellFlyoutHeaderBehavior : TestShell
{
	public ShellFlyoutHeaderBehavior()
	{
	}

	protected override void Init()
	{

		// Lock FlyoutBehavior to keep the flyout open when interacting with menu items, ensuring consistent testing across platforms.
		Shell.SetFlyoutBehavior(this, FlyoutBehavior.Locked);
		FlyoutHeader = new Grid()
		{
			HeightRequest = 143,
			BackgroundColor = Colors.Black,
			AutomationId = "FlyoutHeaderId",
			Children =
			{
				new Image()
				{
					Aspect = Aspect.AspectFill,
					Source = "xamarinstore.jpg",
					Opacity = 0.6
				},
				new Label()
				{
					Margin = new Thickness(0, 40, 0, 0),
					Text="Hello XamStore",
					TextColor=Colors.White,
					FontAttributes=FontAttributes.Bold,
					VerticalTextAlignment = TextAlignment.Center
				}
			}
		};

		for (int i = 0; i < 40; i++)
		{
			AddFlyoutItem(CreateContentPage(), $"Item {i}");
		}

		ContentPage CreateContentPage()
		{
			var page = new ContentPage();
			var layout = new StackLayout();

			foreach (FlyoutHeaderBehavior value in Enum.GetValues(typeof(FlyoutHeaderBehavior)))
			{
				var local = value;
				layout.Add(new Button()
				{
					Text = $"{value}",
					AutomationId = $"{value}",
					Command = new Command(() =>
					{
						this.FlyoutHeaderBehavior = local;
					})
				});
			}

			page.Content = layout;
			return page;
		}
		;
	}
}
