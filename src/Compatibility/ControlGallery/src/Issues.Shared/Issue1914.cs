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
using Microsoft.Maui.Graphics;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1914, "Android rotation ignores anchor", PlatformAffected.Android)]
	public class Issue1914 : ContentPage
	{
		public Issue1914()
		{
			Content = new Rotator();
		}

		class Rotator : AbsoluteLayout
		{
			public Rotator()
			{
				var image = new Image { Aspect = Aspect.AspectFit, Source = "bank.png" };
				Children.Add(image, new Rect(.5, .5, .5, .5), AbsoluteLayoutFlags.All);
				VerticalOptions = HorizontalOptions = LayoutOptions.Center;
				image.RotateTo(3600, 10000);
			}
		}
	}
}

