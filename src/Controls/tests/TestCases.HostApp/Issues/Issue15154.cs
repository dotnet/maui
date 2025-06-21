namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15154, "iOS Flyout title is not broken over multiple lines when you rotate your screen", PlatformAffected.iOS)]
public class Issue15154 : Shell
{
	public Issue15154()
	{
		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Template with behavior with height",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Template Grid",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Template Grid with behavior with height",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Template Grid with behavior without height",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Text ScrollView",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Template ScrollView",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Text ScrollView with behavior with height",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});

		this.Items.Add(new ShellContent
		{
			Title = "EmptyView Text ScrollView without behavior with height",
			ContentTemplate = new DataTemplate(typeof(Issue15154_MainPage))
		});
	}

	public class Issue15154_MainPage : ContentPage
	{
		public Issue15154_MainPage()
		{
			var OpenFlyoutButton = new Button
			{
				Text = "Open Flyout",
				AutomationId = "OpenFlyoutButton"
			};

			OpenFlyoutButton.Clicked += (sender, args) => Shell.Current.FlyoutIsPresented = true;
			Content = new StackLayout()
			{
				Children = { OpenFlyoutButton }
			};
		}
	}
}