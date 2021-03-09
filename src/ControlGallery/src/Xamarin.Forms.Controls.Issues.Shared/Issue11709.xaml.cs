using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11709, "[Bug] [WPF] ScrollView - ScrollBarVisibility not respected ", PlatformAffected.WPF)]
	public partial class Issue11709 : ContentPage
	{
		public Issue11709()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}