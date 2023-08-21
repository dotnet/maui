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
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36479, "[WP8] Picker is not disabled when IsEnabled is set to false", PlatformAffected.WinPhone)]
	public class Bugzilla36479 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker
			{
				IsEnabled = false
			};
			picker.Items.Add("item");
			picker.Items.Add("item 2");

			Content = new StackLayout
			{
				Children =
				{
					picker,
					new Button
					{
						Command = new Command(() =>
						{
							if (picker.IsEnabled)
								picker.IsEnabled = false;
							else
								picker.IsEnabled = true;
						}),
						Text = "Enable/Disable Picker"
					}
				}
			};
		}
	}
}
