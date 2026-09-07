namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35764, "[Android] SearchHandler.ClearPlaceholderEnabled has no effect", PlatformAffected.Android)]
public partial class Issue35764 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		Items.Add(new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue35764ContentPage() { Title = "Home" }
		});
	}

	class Issue35764ContentPage : ContentPage
	{
		public Issue35764ContentPage()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);

			SearchHandler searchHandler = new SearchHandler
			{
				AutomationId = "Issue35764SearchHandler",
				Placeholder = "Search",
				ClearPlaceholderEnabled = true,
				ClearPlaceholderIcon = "bank.png",
			};

			Shell.SetSearchHandler(this, searchHandler);

			Label descriptionLabel = new Label
			{
				Text = "The test passes if the ClearPlaceholder icon respects ClearPlaceholderEnabled visibility.",
				HorizontalTextAlignment = TextAlignment.Center,
			};

			Label statusLabel = new Label
			{
				Text = $"ClearPlaceholderEnabled: {searchHandler.ClearPlaceholderEnabled}",
				AutomationId = "ClearPlaceholderEnabledStatus",
				HorizontalTextAlignment = TextAlignment.Center,
			};

			Button toggleButton = new Button
			{
				Text = "Disable ClearPlaceholder",
				AutomationId = "ToggleClearPlaceholderEnabled",
				HorizontalOptions = LayoutOptions.Center,
			};

			toggleButton.Clicked += (_, _) =>
			{
				searchHandler.ClearPlaceholderEnabled = !searchHandler.ClearPlaceholderEnabled;
				toggleButton.Text = searchHandler.ClearPlaceholderEnabled
					? "Disable ClearPlaceholder"
					: "Enable ClearPlaceholder";
				statusLabel.Text = $"ClearPlaceholderEnabled: {searchHandler.ClearPlaceholderEnabled}";
			};

			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = 20,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children = { descriptionLabel, statusLabel, toggleButton },
			};
		}
	}
}
