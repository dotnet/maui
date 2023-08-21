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
using static Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Application;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3228, "[UWP] Default Search Directory for UWP Icons (Platform Specific)")]
	public partial class Issue3228 : TestContentPage
	{
		public Issue3228()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			Application.Current.On<WindowsOS>().SetImageDirectory("Assets");
		}
	}
}