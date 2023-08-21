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
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39395, "SwitchCell does not take all available place inside ListView",
		PlatformAffected.WinRT)]
	public class Bugzilla39395 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				FontSize = 18,
				Text =
					"The switch cells below should be aligned with the right edge of the screen. If they are not, this test has failed."
			};

			Content = new StackLayout
			{
				BackgroundColor = Colors.Gray,
				Children = {
					instructions,
					new ListView {
						ItemTemplate = new DataTemplate (typeof(SwitchCell)),
						ItemsSource = new[] { "Text", "Text" }
					}
				}
			};
		}
	}
}
