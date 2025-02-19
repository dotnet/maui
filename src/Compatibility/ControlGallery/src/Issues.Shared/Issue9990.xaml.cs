using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9990, "Frame's shadow doesn't translate with the Frame by TranslatTo method (iOS)", PlatformAffected.iOS)]
	public partial class Issue9990 : ContentPage
	{
		public Issue9990()
		{
#if APP
			InitializeComponent();
#endif
		}

#if APP
		int x = 60;
		void BtnMove(object sender, EventArgs e)
		{
			frm.TranslateTo(x, 0);
			x = -x;
		}
#endif
	}
}