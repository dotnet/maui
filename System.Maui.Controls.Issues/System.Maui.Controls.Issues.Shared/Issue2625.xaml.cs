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
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2625, "VisualStateManager attached to Button seems to not work on Android", PlatformAffected.Android)]
	public partial class Issue2625 : ContentPage
	{
		public Issue2625 ()
		{
#if APP
			InitializeComponent ();
#endif
		}
	}
}