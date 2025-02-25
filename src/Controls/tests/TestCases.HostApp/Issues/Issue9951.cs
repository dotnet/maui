namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 9951, "Android 10 Setting ThumbColor on Switch causes a square block", PlatformAffected.Android)]
	public class Issue9951 : TestContentPage
	{
		private const string switchId = "switch";

		public Issue9951()
		{
		}

		protected override void Init()
		{
			var grid = new Grid();

			grid.Children.Add(new Switch()
			{
				ThumbColor = Colors.Red,
				OnColor = Colors.Yellow,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				AutomationId = switchId
			});

			Content = grid;
		}
	}
}
