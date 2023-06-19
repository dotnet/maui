using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8967, "Entry Clear Button miss placement on android", PlatformAffected.Android)]
	public partial class Issue8967 : ContentPage
	{
		public Issue8967()
		{
#if APP
			InitializeComponent();
#endif
		}
	}

}