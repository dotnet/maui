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
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44453, "[UWP] ToolbarItem Text hard to see when BarTextColor is light", PlatformAffected.WinRT)]
	public class Bugzilla44453 : TestFlyoutPage
	{
		protected override void Init()
		{
			var content = new ContentPage
			{
				Title = "UWPToolbarItemColor",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							LineBreakMode = LineBreakMode.WordWrap,
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "The toolbar secondary items should not have white text on a light background"
						}
					}
				}
			};

			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			Flyout = new ContentPage
			{
				Title = "Flyout"
			};
			Detail = new NavigationPage(content)
			{
				BarBackgroundColor = Colors.Green,
				BarTextColor = Colors.White
			};

			Detail.ToolbarItems.Add(new ToolbarItem("Test Secondary Item", null, delegate
			{ }, ToolbarItemOrder.Secondary));
		}
	}
}