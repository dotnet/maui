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
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7048, "[Bug][UWP] CheckBox Has Incosistent Paddings",
		PlatformAffected.UWP)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue7048 : ContentPage
	{
		public Issue7048()
		{
			InitializeComponent();
		}
	}
#endif
}