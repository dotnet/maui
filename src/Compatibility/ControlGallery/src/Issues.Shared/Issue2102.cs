using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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