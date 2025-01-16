namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26466, "Button is not released on unload", PlatformAffected.WinRT)]
	public partial class Issue26466 : TestContentPage
	{
		protected override void Init()
		{
			var button = new Button()
			{
				Text = "Hello",
				AutomationId = "thebutton"
			};

			var success = new Label
			{
				Text = "If you see this, the test has passed",
				AutomationId = "success"
			};

			var layout = new Microsoft.Maui.Controls.StackLayout();
			layout.Children.Add(button);

			button.Pressed += (s, e) =>
			{
				layout.Children.Remove(button);
			};

			button.Released += (s, e) =>
			{
				layout.Children.Add(success);
			};

			Content = layout;
		}
	}
}
