using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2983, "ListView.Footer can cause NullReferenceException", PlatformAffected.iOS)]
	public class Issue2983 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				Footer = new StackLayout
				{
					Children = { new Label { Text = "Footer", AutomationId = "footer" } }
				}
			};
		}
	}
}