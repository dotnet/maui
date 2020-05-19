using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.PlatformConfiguration.WindowsSpecific.Application;
using WindowsOS = Xamarin.Forms.PlatformConfiguration.Windows;

namespace Xamarin.Forms.Controls.Issues
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