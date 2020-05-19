using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
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
