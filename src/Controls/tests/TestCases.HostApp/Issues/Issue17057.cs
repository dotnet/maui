namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17057, "Shell FlowDirection not updating properly", PlatformAffected.Android)]
	public class Issue17057 : Shell
	{
		public Issue17057()
		{
			FlowDirection = FlowDirection.RightToLeft;
			var flyoutItem1 = new FlyoutItem { Title = "Item1" };
			var tab1 = new Tab();
			tab1.Items.Add(new ShellContent { ContentTemplate = new DataTemplate(typeof(_17057Page)) });
			flyoutItem1.Items.Add(tab1);
			Items.Add(flyoutItem1);
		}
	}

	public class _17057Page : ContentPage
	{
		public _17057Page()
		{
			Content = new Label
			{
				Text = "This is a Label",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				AutomationId = "label"
			};
		}
	}
}
