namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1219, "Setting ToolbarItems in ContentPage constructor crashes app", PlatformAffected.iOS)]
	public class Issue1219 : TestContentPage
	{
		const string Success = "Success";

		public Issue1219()
		{
			Content = new Label { Text = Success };

			ToolbarItems.Add(new ToolbarItem("MenuItem", "", () =>
			{

			}));
		}

		protected override void Init() { }
	}
}