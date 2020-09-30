using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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