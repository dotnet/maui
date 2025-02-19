using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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