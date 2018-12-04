using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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