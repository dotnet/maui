using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 7329, "[Android] ListView scroll not working when inside a ScrollView",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.ScrollView)]
#endif
	public class Issue7329 : TestContentPage
	{
		ListView listView = null;
		protected override void Init()
		{
			listView = new ListView() { AutomationId = "NestedListView" };
			listView.ItemsSource = Enumerable.Range(0, 200).Select(x => new Data() { Text = x }).ToList();

			Content = new ScrollView()
			{
				AutomationId = "ParentScrollView",
				Content = new StackLayout()
				{
					Children =
					{
						new ApiLabel(),
						new Label() { Text = "If the List View can scroll the test has passed"},
						listView
					}
				}
			};
		}

		[Preserve(AllMembers = true)]
		public class Data
		{
			public int Text { get; set; }

			public override string ToString()
			{
				return Text.ToString();
			}
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void ScrollListViewInsideScrollView()
		{

#if __ANDROID__
			if (!RunningApp.IsApiHigherThan(21))
				return;
#endif

			RunningApp.WaitForElement("1");

			RunningApp.QueryUntilPresent(() =>
			{
				try
				{
					RunningApp.ScrollDownTo("30", strategy: ScrollStrategy.Gesture, swipeSpeed: 100);
				}
				catch
				{
					// just ignore if it fails so it can keep trying to scroll
				}

				return RunningApp.Query("30");
			});

			RunningApp.Query("30");
		}
#endif
	}
}
