using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11259, "[Bug] UWP ScrollView Entry's text is blank until a tap on it", PlatformAffected.UWP)]
	public partial class Issue11259
	{
		public Issue11259()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}
