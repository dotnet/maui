namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Bugzilla, 60123, "Rui's issue", PlatformAffected.Default)]
	public class Bugzilla60123 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			var items = new List<string>();
			for (int i = 0; i < 100; i++)
			{
				items.Add(i.ToString());
			}

			var listView = new ListView(ListViewCachingStrategy.RecycleElement)
			{
				BackgroundColor = Colors.Yellow,
				AutomationId = "ListView"
			};

			listView.ItemsSource = items;

			Content = listView;
		}
	}
}