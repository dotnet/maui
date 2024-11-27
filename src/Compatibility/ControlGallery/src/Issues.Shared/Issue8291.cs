using System.Linq;
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
	[Issue(IssueTracker.Github, 8291, "[Android] Editor - Text selection menu does not appear when selecting text on an editor placed within a ScrollView",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Editor)]
#endif
	public class Issue8291 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Only Relevant on Android"
					},
					new ScrollView()
					{
						Content = new Editor()
						{
							Text = "Press and hold this text. Text should become selected and context menu should open",
							AutomationId = "PressEditor"
						}
					},
					new ScrollView()
					{
						Content = new Entry()
						{
							Text = "Press and hold this text. Text should become selected and context menu should open",
							AutomationId = "PressEntry"
						}
					}
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void ContextMenuShowsUpWhenPressAndHoldTextOnEditorAndEntryField()
		{
			RunningApp.TouchAndHold("PressEditor");
			TestForPopup();
			RunningApp.Tap("PressEntry");
			RunningApp.TouchAndHold("PressEntry");
			TestForPopup();
		}

		void TestForPopup()
		{
			var result = RunningApp.QueryUntilPresent(() =>
			{
				return RunningApp.Query("Paste")
						.Union(RunningApp.Query("Share"))
						.Union(RunningApp.Query("Copy"))
						.Union(RunningApp.Query("Cut"))
						.Union(RunningApp.Query("Select All"))
						.ToArray();
			});

			Assert.IsNotNull(result);
			Assert.IsTrue(result.Length > 0);
		}
#endif
	}
}
