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
	[Issue(IssueTracker.Github, 6693, "[Bug] [WPF] ViewRenderer does not work properly with FrameworkElement derived native controls", PlatformAffected.WPF)]
	public partial class Issue6693 : ContentPage
	{
		public Issue6693()
		{
#if APP
			InitializeComponent();
#endif
		}
	}

	public class Issue6693Control : View
	{

	}
}