using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39463, "Items not showing in ListView using DataTemplate when there are more items than can fit on screen", PlatformAffected.WinPhone)]
	public partial class Bugzilla39463 : ContentPage
	{
		public Bugzilla39463()
		{
#if !UITEST
			InitializeComponent();

			var n = 16;

			var listStr = new List<string>();
			for (int i = 0; i < n; i++)
			{
				listStr.Add("Test : " + i);
			}

			lvView.ItemsSource = listStr;
#endif
		}
	}
}
