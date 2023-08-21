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

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2838, "UWP does not render Frame CornerRadius", PlatformAffected.UWP)]
	public class Issue2838 : TestContentPage
	{
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			Content = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				Children =
				{
					new Label()
					{
						Text ="The frame below should have its corners rounded and the background should not protrude through them.",
						TextColor = Colors.Black,
						WidthRequest = 300,
						HeightRequest = 90,
						LineBreakMode = LineBreakMode.WordWrap,
						HorizontalOptions = LayoutOptions.Center,
						Margin = new Thickness(10)
					},
					new Frame
					{
						WidthRequest = 300,
						HeightRequest = 160,
						HorizontalOptions = LayoutOptions.Center,
						CornerRadius = 10,
						BackgroundColor = Colors.Red,
						BorderColor = Colors.Blue
					}
				}
			};
		}
	}
}