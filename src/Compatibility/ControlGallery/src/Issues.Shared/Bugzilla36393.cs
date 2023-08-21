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

using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
