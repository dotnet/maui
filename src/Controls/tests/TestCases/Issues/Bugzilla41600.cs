using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
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
	}
}