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
using System.Diagnostics;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 229, "ToolbarItems broken", PlatformAffected.Android)]
	public class Issue229 : ContentPage
	{
		public Issue229()
		{
			Title = "I am a navigation page.";

			var label = new Label
			{
				Text = "I should have a toolbar item",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center
			};

			var refreshBtn = new ToolbarItem("Refresh", null, () => label.Text = "Clicking it works");

			ToolbarItems.Add(refreshBtn);

			Content = label;
		}
	}
}
