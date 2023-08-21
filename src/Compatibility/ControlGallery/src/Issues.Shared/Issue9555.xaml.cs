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
	public class FooEffect : RoutingEffect
	{
		public FooEffect() : base("XamControl.FooEffect")
		{
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9555, "[iOS] Applying an Effect to Frame adds a shadow", PlatformAffected.iOS)]
	public partial class Issue9555 : ContentPage
	{
		public Issue9555()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}