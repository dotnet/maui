using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Xaml;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
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