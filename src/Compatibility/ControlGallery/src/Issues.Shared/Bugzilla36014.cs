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

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36014, "Picker Control Is Not Rendered Correctly", PlatformAffected.WinPhone)]
	public class Bugzilla36014 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker { Items = { "Leonardo", "Donatello", "Raphael", "Michaelangelo" } };
			var label = new Label
			{
				Text =
					"This test is successful if the picker below spans the width of the screen. If the picker is "
					+ "just a sliver on the left edge of the screen, this test has failed."
			};

			Content = new StackLayout { Children = { label, picker } };
		}
	}
}
