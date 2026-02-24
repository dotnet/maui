namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20250, "[iOS] SearchHandler ClearPlaceholderIcon color", PlatformAffected.iOS)]
public partial class Issue20250 : TestShell
{

	protected override void Init()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue20250ContentPage() { Title = "Home" }
		};

		Items.Add(shellContent);

	}
	class Issue20250ContentPage : ContentPage
	{
		public Issue20250ContentPage()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Shell.SetSearchHandler(this, new SearchHandler
			{
				AutomationId = "searchHandler",
				Placeholder = "Search",
				TextColor = Colors.Red,
				ClearPlaceholderEnabled = true,
				ClearPlaceholderIcon = "bank.png",

			});

			this.SetBinding(TitleProperty, new Binding("Title", source: Shell.Current));

			Content = new Label
			{
				Text = "SearchHandler",
				AutomationId = "label",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};
		}
	}
}