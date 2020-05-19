using System;
using System.Collections.Generic;
using System.Text;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 41205, "UWP CreateDefault passes string instead of object")]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Bugzilla41205 : TestContentPage
	{
		const string _success = "Pass";

		[Preserve(AllMembers = true)]
		public class ViewModel
		{
			public string Text { get { return _success; } }
		}

		[Preserve(AllMembers = true)]
		public class CustomListView : ListView
		{
			protected override Cell CreateDefault(object item)
			{
				if (item is ViewModel)
				{
					var newTextCell = new TextCell();
					newTextCell.SetBinding(TextCell.TextProperty, nameof(ViewModel.Text));
					return newTextCell;
				}
				return base.CreateDefault("Fail");
			}
		}

		protected override void Init()
		{
			var listView = new CustomListView
			{
				ItemsSource = new[]
				{
					new ViewModel(),
					new ViewModel(),
				}
			};

			Content = listView;
		}

#if UITEST
		[Test]
		public void CreateDefaultPassesStringInsteadOfObject()
		{
			RunningApp.WaitForElement(_success);
		}
#endif

	}
}
