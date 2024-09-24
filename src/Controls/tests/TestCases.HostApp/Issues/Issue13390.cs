using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13390, "Custom SlideFlyoutTransition is not working",
		PlatformAffected.iOS)]
	public class Issue13390 : TestShell
	{
		protected override void Init()
		{
			CreateContentPage()
				.Content = new Label()
				{
					Text = "If app has not crashed test has passed",
					AutomationId = "Success"
				};
		}
	}
}
