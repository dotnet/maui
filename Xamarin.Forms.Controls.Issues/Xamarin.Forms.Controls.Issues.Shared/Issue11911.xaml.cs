using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11911, "[Bug] Brushes can't use DynamicResource", PlatformAffected.All)]
	public partial class Issue11911 : ContentPage
	{
		public Issue11911()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}