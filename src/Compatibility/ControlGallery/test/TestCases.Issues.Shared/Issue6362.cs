using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6362, "[iOS] Shell GoToAsync doesn't update selected tab to bold",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue6362 : TestShell
	{
		protected override void Init()
		{
			Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);
			AddTopTab(
				new ContentPage()
				{
					Content = new StackLayout()
					{
						Children =
						{
							new Label()
							{
								Text = "Click between tabs and make sure the tab colors change"
							},
							new Button()
							{
								Text = "Go to tab 2 and make sure tab colors change",
								Command = new Command(()=>
								{
									GoToAsync(@"\\\Second");
								})
							}
						}
					}
				},
				"First");

			AddTopTab("Second");
		}
	}
}
