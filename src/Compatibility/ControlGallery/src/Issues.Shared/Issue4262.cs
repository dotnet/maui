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
	[Issue(IssueTracker.Github, 4262, "Label HorizontalTextAlignment=\"Center\" not working in conjunction with LineHeight on iOS", PlatformAffected.iOS)]
	public class Issue4262 : ContentPage
	{
		public Issue4262()
		{
			var label = new Label() { Text = "This is center aligned&#x0a;line 2.", HorizontalTextAlignment = TextAlignment.Center };
			var label2 = new Label() { Text = "If this is not center aligned, this test has failed.", HorizontalTextAlignment = TextAlignment.Center, LineHeight = 1.5 };

			Content = new StackLayout()
			{
				Children = { label, label2 },
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
		}
	}
}