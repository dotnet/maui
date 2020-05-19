using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Xaml;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6130, "[Bug] [Forms 4.0] [Android] Mismatch in WebView.EnableZoomControls platform-specific", PlatformAffected.Android)]
	public partial class Issue6130 : ContentPage
	{
		public Issue6130()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}