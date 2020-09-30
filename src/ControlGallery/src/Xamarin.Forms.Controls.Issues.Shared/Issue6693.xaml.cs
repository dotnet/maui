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