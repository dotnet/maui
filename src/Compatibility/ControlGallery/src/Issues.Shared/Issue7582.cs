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
	[Issue(IssueTracker.Github, 7582, "Null Reference Exception thrown when set FontFamily for label in Xamarin Forms macOS", PlatformAffected.macOS)]
	public class Issue7582 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label
			{
				Text = "This is a label with a non-existent FontFamily, If you're seeing this label the test succeeded.",
				FontFamily = "THISISNOTAFONT",
				FontAttributes = FontAttributes.Bold,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			Content = label;
		}
	}
}