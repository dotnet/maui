//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11911, "[Bug] Brushes can't use DynamicResource", PlatformAffected.All)]
	public partial class Issue11911 : ContentPage
	{
		public Issue11911()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}