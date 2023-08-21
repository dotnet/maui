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
	[Issue(IssueTracker.Github, 4194, "[iOS] SearchBar changes color if initial state is invisible", PlatformAffected.iOS)]

	public partial class Issue4194 : ContentPage
	{
		public Issue4194()
		{
#if APP
			InitializeComponent();
#endif
		}

		void Handle_Clicked(object sender, System.EventArgs e)
		{
#if APP
			SearchBar1.IsVisible = true;
#endif
		}
	}
}