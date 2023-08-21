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
	[Issue(IssueTracker.Bugzilla, 48236, "[WinRT/UWP] BackgroundColor for Stepper behaves differently compared to iOS to Android", PlatformAffected.WinRT)]
	public class Bugzilla48236 : TestContentPage
	{
		protected override void Init()
		{
			var stepper = new Stepper
			{
				BackgroundColor = Colors.Green,
				Minimum = 0,
				Maximum = 10
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "If the Stepper's background color extends the width of the page, then this test has failed."
					},
					stepper,
					new Button
					{
						BackgroundColor = Colors.Aqua,
						Text = "Change Stepper Color to Yellow",
						Command = new Command(() =>
						{
							stepper.BackgroundColor = Colors.Yellow;
						})
					}
				}
			};
		}
	}
}