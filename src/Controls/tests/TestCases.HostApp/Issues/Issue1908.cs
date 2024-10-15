namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1908, "Image reuse", PlatformAffected.Android)]
	public class Issue1908 : TestContentPage
	{

		public Issue1908()
		{

		}

		protected override void Init()
		{
			StackLayout listView = new StackLayout();

			for (int i = 0; i < 1000; i++)
			{
				listView.Children.Add(new Image() { Source = "oasis.jpg", ClassId = $"OASIS{i}", AutomationId = $"OASIS{i}" });
			}

			Content = new ScrollView() { Content = listView };
		}
	}
}
