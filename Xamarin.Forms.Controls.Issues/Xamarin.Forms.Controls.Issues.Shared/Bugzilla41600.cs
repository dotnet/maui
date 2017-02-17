using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41600, "[Android] Invalid item param value for ScrollTo throws an error", PlatformAffected.Android)]
	public class Bugzilla41600 : TestContentPage
	{
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
						Text = "Click for ScrollTo (should do nothing)",
						Command = new Command(() =>
						{
							listView.ScrollTo("Hello", ScrollToPosition.Start, true);
						})
					},
					new Button
					{
						Text = "Click for ScrollTo (should go to 15)",
						Command = new Command(() =>
						{
							listView.ScrollTo(items[15], ScrollToPosition.Start, false);
						})
					}
				}
			};
		}
	}
}