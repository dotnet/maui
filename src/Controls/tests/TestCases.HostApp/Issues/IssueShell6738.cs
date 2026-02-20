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
						new Button
						{
							AutomationId = "IconColorChangeButton",
							Text = "Change Icon Color",
							Command = new Command(() => UpdateFlyoutIconColor(this))
						},

						new Button
						{
							AutomationId = "IconColorDefaultButton",
							Text = "Change to Default Icon Color",
							Command = new Command(() => UpdateDefaultFlyoutIconColor(this))
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

		private void UpdateFlyoutIconColor(Shell shell)
		{
			SetForegroundColor(shell, Colors.Green);
		}

		void UpdateDefaultFlyoutIconColor(Shell shell)
		{
			SetForegroundColor(shell, null);
		}
	}
}