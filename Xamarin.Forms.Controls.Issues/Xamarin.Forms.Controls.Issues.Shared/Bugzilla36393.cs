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
	[Issue(IssueTracker.Bugzilla, 36393, "[A] Default Entry/Editor/SearchBar Font Size is 14 instead of 18")]
	public class Bugzilla36393 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var label = new Label { FontSize = 18 };
			var entry = new Entry { };
			var editor = new Editor { };
			var searchBar = new SearchBar { };

			label.Text = $"I am label. FontSize:{label.FontSize}";
			entry.Text = $"I am entry. FontSize:{entry.FontSize}";
			editor.Text = $"I am editor. FontSize:{editor.FontSize}";
			searchBar.Text = $"I am search bar. FontSize:{searchBar.FontSize}";

			// Initialize ui here instead of ctor
			Content = new StackLayout
			{
				Children = { label, entry, editor, searchBar }
			};
		}

#if UITEST
		[Test]
		public void Issue1Test()
		{
			RunningApp.Screenshot("If all of the font sizes are visibly the same size, this test has passed.");
		}
#endif
	}
}
