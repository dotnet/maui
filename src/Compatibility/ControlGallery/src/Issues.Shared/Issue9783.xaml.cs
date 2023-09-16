using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9783, "DatePicker begin to cause Exception in version 4.5.0-282-pre4 and above in some situations",
		PlatformAffected.UWP)]
	public partial class Issue9783 : TestContentPage
	{
		public Issue9783()
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