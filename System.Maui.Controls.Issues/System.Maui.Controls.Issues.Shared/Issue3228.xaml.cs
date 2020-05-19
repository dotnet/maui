using System.Maui.CustomAttributes;
using System.Maui.Internals;
using static System.Maui.PlatformConfiguration.WindowsSpecific.Application;
using WindowsOS = System.Maui.PlatformConfiguration.Windows;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3228, "[UWP] Default Search Directory for UWP Icons (Platform Specific)")]
	public partial class Issue3228 : TestContentPage
	{
		public Issue3228()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			Application.Current.On<WindowsOS>().SetImageDirectory("Assets");
		}
	}
}