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

using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41600, "[Android] Invalid item param value for ScrollTo throws an error", PlatformAffected.Android)]
	public class Bugzilla41600 : TestContentPage
	{
		const string _btnScrollToNonExistentItem = "btnScrollToNonExistentItem";
		const string _btnScrollToExistentItem = "btnScrollToExistentItem";
		const string _firstListItem = "0";
		const string _middleListItem = "15";

		protected override void Init()
		{
			var items = new List<string>();
			for (var i = 0; i <= 30; i++)
				items.Add(i.ToString());

			var listView = new ListView
			{
				ItemsSource = items
			};
			Content = new StackLayout
			{
				Children =
				{
					listView,
					new Button
					{
						AutomationId = _btnScrollToNonExistentItem,
						Text = "Click for ScrollTo (should do nothing)",
						Command = new Command(() =>
						{
							listView.ScrollTo("Hello", ScrollToPosition.Start, true);
						})
					},
					new Button
					{
						AutomationId = _btnScrollToExistentItem,
						Text = "Click for ScrollTo (should go to 15)",
						Command = new Command(() =>
						{
							listView.ScrollTo(_middleListItem, ScrollToPosition.Start, false);
						})
					}
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla41600Test()
		{
			RunningApp.WaitForElement(_btnScrollToNonExistentItem);
			RunningApp.WaitForElement(_btnScrollToExistentItem);
			
			RunningApp.Tap(_btnScrollToNonExistentItem);
			RunningApp.WaitForElement(_firstListItem);
			RunningApp.WaitForNoElement(_middleListItem);

			RunningApp.Tap(_btnScrollToExistentItem);
			RunningApp.WaitForNoElement(_firstListItem);
			RunningApp.WaitForElement(_middleListItem);
		}
#endif
	}
}