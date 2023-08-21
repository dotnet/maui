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
	[Issue(IssueTracker.Bugzilla, 40998, "[UWP] Pressing escape with an awaited DisplayActionSheet doesn't return a result", PlatformAffected.WinRT)]
	public class Bugzilla40998 : TestContentPage
	{
		protected override void Init()
		{
			var resultLabel = new Label
			{
				Text = "ActionSheet Result - use the ActionSheet to show the result"
			};
			Content = new StackLayout
			{
				Children =
				{
					resultLabel,
					new Button
					{
						Text = "Click to display ActionSheet",
						Command = new Command(async () =>
						{
							var result = await DisplayActionSheet("Test ActionSheet", "Cancel", "Destroy", new string[] { "Test Button" });
							resultLabel.Text = result;
						})
					}
				}
			};
		}
	}
}
