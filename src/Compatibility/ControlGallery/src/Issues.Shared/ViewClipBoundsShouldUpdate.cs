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

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 888888, "Bounds clipping does not update when View bounds change", PlatformAffected.Android)]
	public class ViewClipBoundsShouldUpdate : TestContentPage
	{
		const string Success = "Success";

		class TestContentView : ContentView
		{
			public TestContentView()
			{
				Content = new Label { Text = Success };

				IsClippedToBounds = true;
			}
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = $"If '{Success}' displays below then this test has passed."
					},
					new TestContentView()
				}
			};

			Content = layout;
		}

	}
}