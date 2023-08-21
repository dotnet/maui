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

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Issue(IssueTracker.Bugzilla, 28939, " Entry Control loses cursor position to either beginning or end of input ",
		PlatformAffected.WinPhone)]
	public class Bugzilla28939 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label {
						Text = @"Enter the text ""testing"" in the Entry Control below. Move the cursor between the 'e' and the 's'. Type the letter 'a'. If the cursor is positioned after the 'a', the test has passed."
					},
					new Entry()
				}
			};
		}
	}
}
