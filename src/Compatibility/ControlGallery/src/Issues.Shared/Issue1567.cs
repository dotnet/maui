﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1567, "NRE in NavigationProxy.set_Inner", PlatformAffected.iOS)]
	public class Issue1567
		: ContentPage
	{
		public Issue1567()
		{
			Title = "Test";
			var label = new Label { Text = "Whatever" };

			ToolbarItems.Add(new ToolbarItem("Modal", null, async () =>
			{
				var cp2 = new ContentPage()
				{
					Title = "Modal",
					Content = new Label() { Text = "Second screen" },
				};
				//var np2 = new NavigationPage(cp2) { Title = "Modal" };
				try
				{
					await Navigation.PushModalAsync(cp2);
				}
				catch (InvalidOperationException ex)
				{
					label.Text = "Exception properly thrown: " + ex.Message;
				}
			}));

			Content = label;
		}
	}
}
