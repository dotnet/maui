using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.Issues
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