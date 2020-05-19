using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
{
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6403, "System.Maui UWP Picker collapses on opening Dropdown menu [Bug]", PlatformAffected.UWP)]
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