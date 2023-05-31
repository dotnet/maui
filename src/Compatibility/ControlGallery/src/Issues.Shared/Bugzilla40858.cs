using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40858, "Long clicking a text entry in a ListView header/footer cause a crash", PlatformAffected.Android)]
	public class Bugzilla40858 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new ListView
					{
						Header = new Editor
						{
							AutomationId = "Header",
							HeightRequest = 50,
							Text = "ListView Header -- Editor"
						},
						Footer = new Entry
						{
							AutomationId = "Footer",
							HeightRequest = 50,
							Text = "ListView Footer -- Entry"
						}
					}
				}
			};
		}

#if UITEST

#if __ANDROID__
		[Test]
		public void ListViewDoesNotCrashOnTextEntryHeaderOrFooterLongClick()
		{
			RunningApp.TouchAndHold(x => x.Marked("Header"));
			RunningApp.TouchAndHold(x => x.Marked("Footer"));
		}
#endif

#endif
	}
}
