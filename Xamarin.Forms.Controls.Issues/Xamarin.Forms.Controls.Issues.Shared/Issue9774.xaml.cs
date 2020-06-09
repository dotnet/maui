using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{
	public class CustomFrame9974 : Frame
	{
		public CustomFrame9974()
		{
		}
	}

	[Issue(IssueTracker.Github, 9774, "Xamarin.Forms 4.5 breaks custom frame renderer shadow functionality on iOS",
		PlatformAffected.iOS)]
	public partial class Issue9774 : TestContentPage
	{
		public Issue9774()
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