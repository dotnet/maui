using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39829, "RowHeight of ListView is not working for UWP", PlatformAffected.UWP)]
	public class Bugzilla39829 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Master";

			var instructions = new Label 
			{ 
				Text = "The text items in the list below should be spaced far apart vertically. " 
					+ "If they are close together, this test has failed."

			};

			var listView = new ListView
			{
				RowHeight = 150,
				AutomationId = "listview",
				ItemsSource = new[] { "Test1", "Test2", "Test3", "Test4", "Test5", "Test6", }
			};

			Content = new StackLayout { Children = { instructions, listView } };
		}
	}
}
