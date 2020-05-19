using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2102, "Empty NavigationPage throws NullReferenceException", PlatformAffected.UWP)]
	public class Issue2102 : TestTabbedPage
	{
		protected override void Init()
		{
			Children.Add(new NavigationPage { Title = "Success" });
		}
	}
}