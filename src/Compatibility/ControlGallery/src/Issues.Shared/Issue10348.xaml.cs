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
	public class CustomFrame10348 : Frame
	{

	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10348, "[Bug] Cannot change shadow appearance on Frame from version 4.5.x", PlatformAffected.iOS)]
	public partial class Issue10348 : ContentPage
	{
		public Issue10348()
		{
#if APP
			InitializeComponent();
#endif
		}
	}

}