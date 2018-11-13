using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4194, "[iOS] SearchBar changes color if initial state is invisible", PlatformAffected.iOS)]

	public partial class Issue4194 : ContentPage
	{
		public Issue4194()
		{
#if APP
			InitializeComponent();
#endif
		}

		void Handle_Clicked(object sender, System.EventArgs e)
		{
#if APP
			SearchBar1.IsVisible = true;
#endif
		}
	}
}