namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 18423, "[Windows] Shell flyout template items do not have a margin applied on first show", PlatformAffected.UWP)]
	public partial class Issue18423Shell : Shell
	{
		public Issue18423Shell()
		{
			FlyoutBehavior = FlyoutBehavior.Flyout;

			ItemTemplate = new DataTemplate(() =>
			{
				return new Label
				{
					AutomationId = "LabelWithMargin",
					Margin = new Thickness(20),
					Text = "Title"
				};
			});

			ShellContent shellContent = new ShellContent
			{
				Title = "Home",
				ContentTemplate = new DataTemplate(typeof(Issue18423Page)),
				Route = "MainPage"
			};

			Items.Add(shellContent);
		}
	}

	public partial class Issue18423Page : ContentPage
	{
		public Issue18423Page()
		{
			BackgroundColor = Colors.HotPink;
			this.Content = new Label() { AutomationId = "MainPageLabel", Text = "Content" };
		}
	}
}