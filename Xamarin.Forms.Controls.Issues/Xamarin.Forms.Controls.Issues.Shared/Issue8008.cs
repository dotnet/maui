using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8008, "Removing Shell Item can cause Shell to try and set a MenuItem as the default visible item")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue8008 : TestShell
	{
		ShellItem item1;
		protected override void Init()
		{
			item1 = AddContentPage();

			item1.Title = "Not Visible";
			Items.Add(new MenuShellItem(new MenuItem()
			{
				Text = "Menu Item",
				Command = new Command(() =>
				{
					throw new Exception("I shouldn't execute after removing an item");
				})
			}));

			var item2 = AddContentPage(new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "If you are reading this then this test has passed",
							AutomationId = "Success"
						}
					}
				}
			});

			item2.Title = "Visible After Remove";
			Device.BeginInvokeOnMainThread(() =>
			{
				this.Items.Remove(item1);
			});

		}

#if UITEST
		[Test]
		public void RemovingShellItemCorrectlyPicksNextValidShellItemAsVisibleShellItem()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
