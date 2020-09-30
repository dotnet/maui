using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6403, "Xamarin.Forms UWP Picker collapses on opening Dropdown menu [Bug]", PlatformAffected.UWP)]
	public partial class Issue6403 : TestContentPage
	{

		public Issue6403()
		{
#if APP
			InitializeComponent();
#endif
		}


		protected override void Init()
		{

		}
	}
}