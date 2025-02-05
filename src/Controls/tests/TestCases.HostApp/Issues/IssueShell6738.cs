namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 6738, "The color of the custom icon in Shell always resets to the default blue", PlatformAffected.iOS | PlatformAffected.macOS)]
	public class IssueShell6738 : TestShell
	{
		protected override void Init()
		{
			FlyoutBehavior = FlyoutBehavior.Flyout;
			FlyoutIcon = "star_flyout.png";

			ContentPage contentPage = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							AutomationId = "Label",
							Text = "ContentPage Label"
						}
					}
				}
			};

			Items.Add(new ShellContent
			{
				Title = "Home",
				Content = contentPage
			});
		}
	}
}