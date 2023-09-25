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