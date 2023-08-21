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
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2625, "VisualStateManager attached to Button seems to not work on Android", PlatformAffected.Android)]
	public partial class Issue2625 : ContentPage
	{
		public Issue2625()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}