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

using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2628, "Unable to change page BackgroundImage from code", PlatformAffected.Android)]
	public class Issue2628 : ContentPage
	{
		public Issue2628()
		{
			var button1 = new Button { Text = "Click !!!!!!!!!!" };
			BackgroundImageSource = "bank.png";
			button1.Clicked += ButtonAction;

			Content = new StackLayout
			{
				Spacing = 10,
				VerticalOptions = LayoutOptions.Center,
				Children = {
					button1
				}
			};
		}

		public void ButtonAction(object sender, EventArgs args)
		{
			BackgroundImageSource = "calculator.png";
		}
	}
}

