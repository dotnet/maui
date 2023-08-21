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
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1025, "StackLayout broken when image missing", PlatformAffected.iOS, NavigationBehavior.PushModalAsync)]
	public class Issue1025 : ContentPage
	{
		public Issue1025()
		{
			var instructions = new Label
			{
				Text = "The StackLayout below should contain two buttons and some text. " +
				"If the StackLayout appears empty, this test has failed."
			};

			var layout = new StackLayout
			{
				BackgroundColor = Color.FromArgb("#dae1eb"),
				Orientation = StackOrientation.Vertical,
				Children = {
					new Image {},
					new Label {Text = "Lorem ipsum dolor" },
					new Label {Text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."},
					new Button {BackgroundColor = Color.FromArgb ("#fec240"), Text = "Create an account" },
					new Button {BackgroundColor = Color.FromArgb ("#04acdb"), Text = "Login" },
				}
			};

			Content = new StackLayout
			{
				Children = { instructions, layout }
			};
		}
	}
}
