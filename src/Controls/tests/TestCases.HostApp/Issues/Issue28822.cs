namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 28822, "ToolbarItem behavior with ImageSource iOS", PlatformAffected.iOS)]
	public class Issue28822NavigationPage : NavigationPage
	{
		public Issue28822NavigationPage() : base(new Issue28822()) { }

		public class Issue28822 : ContentPage
		{
			public Issue28822()
			{
				ToolbarItems.Add(new ToolbarItem()
				{
					IconImageSource = ImageSource.FromFile("dotnet_bot.png"),
				});

				ToolbarItems.Add(new ToolbarItem()
				{
					IconImageSource = new FontImageSource()
					{
						Glyph = "+",
						Color = Colors.Red,
						Size = 20,
					}
				});

				Content = new Label()
				{
					Text = "Hello, World!",
					AutomationId = "HelloWorldLabel"
				};
			}
		}
	}
}
