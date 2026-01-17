namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26158, "SelectionLength property not applied when an entry is focused", PlatformAffected.Android)]
	public class Issue26158 : ContentPage
	{
		public Issue26158()
		{
			Entry entry = new Entry()
			{
				AutomationId = "entry",
				Text = "Microsoft Maui Entry",
			};

			entry.Focused += (s, e) =>
			{
				entry.CursorPosition = 0;
				entry.SelectionLength = 4;
			};

			Content = entry;
		}
	}
}