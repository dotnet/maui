using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;

#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 38989, "[Android] NullReferenceException when using a custom ViewCellRenderer ",
		PlatformAffected.Android)]
	public class Bugzilla38989 : TestContentPage
	{
		const string Success = "If you can see this, the test passed.";

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla38989Test()
		{
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif

		protected override void Init()
		{
			var successLabel = new Label { Text = Success };

			var lv = new ListView();
			var items = new List<string> { "data", "does not", "matter" };

			lv.ItemTemplate = new DataTemplate(typeof(_38989CustomViewCell));

			lv.ItemsSource = items;

			Content = new StackLayout { Children = { successLabel, lv } };
		}

		[Preserve(AllMembers = true)]
		public class _38989CustomViewCell : ViewCell
		{
			public _38989CustomViewCell()
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");

				View = label;
			}
		}
	}
}