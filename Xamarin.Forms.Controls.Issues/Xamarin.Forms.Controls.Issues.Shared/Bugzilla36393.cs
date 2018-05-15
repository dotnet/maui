using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36393, "[A] Default Entry/Editor/SearchBar Font Size is 14 instead of 18")]
	public class Bugzilla36393 : TestContentPage 
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text =
					"If running on Android, all of the Label, Entry, Editor, and SearchBar text sizes below should be"
					+ " the same size."
					+ " If they are not, the test has failed."
					+ " This test should be ignored on non-Android platforms."
			};

			var label = new Label { FontSize = 18 };
			var entry = new Entry();
			var editor = new Editor();
			var searchBar = new SearchBar();

			label.Text = $"I am label. FontSize:{label.FontSize}";
			entry.Text = $"I am entry. FontSize:{entry.FontSize}";
			editor.Text = $"I am editor. FontSize:{editor.FontSize}";
			searchBar.Text = $"I am search bar. FontSize:{searchBar.FontSize}";

			Content = new StackLayout
			{
				Children = { instructions, label, entry, editor, searchBar }
			};
		}
	}
}
