using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
using Xamarin.UITest.Queries;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Editor)]
	[Category(UITestCategories.Entry)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 36171, "WinRT Entry UI not updating on TextChanged",
		PlatformAffected.WinPhone | PlatformAffected.WinRT)]
	public class Bugzilla36171 : TestContentPage 
	{
		protected override void Init ()
		{
			var entry = new Entry { AutomationId = "36171Entry" };
			var editor = new Editor();
			var focuseEntryButton = new Button { Text = "Start Entry" };
			var focuseEditorButton = new Button { Text = "Start Editor" };

			focuseEntryButton.Clicked += (sender, args) => { entry.Focus (); };
			focuseEditorButton.Clicked += (sender, args) => { editor.Focus (); };

			var entryLabel = new Label { Text = "Type 123A into the Entry below; the entry should display '123'. If the 'A' is displayed, the test has failed. If the cursor resets to the beginning of the Entry after typing 'A', the test has failed." };
			var editorLabel = new Label { Text = "Type 123A into the Editor below; the entry should display '123'. If the 'A' is displayed, the test has failed. If the cursor resets to the beginning of the Editor after typing 'A', the test has failed." };

			entry.TextChanged += (sender, args) => {
				var e = sender as Entry;

				int val;

				if(string.IsNullOrEmpty (args.NewTextValue?.Trim ())) {
					return;
				}

				// check if this is numeric
				if(!int.TryParse (args.NewTextValue, out val)) {
					// put the old value back.
					e.Text = args.OldTextValue;
				}
			};

			editor.TextChanged += (sender, args) => {
				var e = sender as Editor;

				int val;

				if(string.IsNullOrEmpty (args.NewTextValue?.Trim ())) {
					return;
				}

				// check if this is numeric
				if(!int.TryParse (args.NewTextValue, out val)) {
					// put the old value back.
					e.Text = args.OldTextValue;
				}
			};

			// Initialize ui here instead of ctor
			Content = new StackLayout {
				Children = { focuseEntryButton, entryLabel, entry, focuseEditorButton, editorLabel, editor }
			};
		}

#if UITEST
		[Test]
#if __MACOS__
		[Ignore("Missing UITest for focus")]
#endif
		public void EntryTextDoesNotDisplayNonnumericInput ()
		{
			RunningApp.WaitForElement ("Start Entry");
			RunningApp.Tap ("Start Entry");

			RunningApp.EnterText ("123A");

			var entry = RunningApp.Query (q => q.Text("123"));
			Assert.That(entry.Length, Is.GreaterThanOrEqualTo(1), "The entry text should be '123'.");

			var failedEntry = RunningApp.Query (q => q.Text("123A"));
			Assert.That(failedEntry.Length, Is.EqualTo(0), "The entry text should only be '123'.");
				
			RunningApp.EnterText ("4");

			var entry2 = RunningApp.Query (q => q.Text("1234"));
			Assert.That(entry2.Length, Is.GreaterThanOrEqualTo(1), "The entry text should now be '1234'.");
		}

		[Test]
#if __MACOS__
		[Ignore("Missing UITest for focus")]
#endif
		public void EditorTextDoesNotDisplayNonnumericInput ()
		{
			RunningApp.WaitForElement ("Start Editor");
			RunningApp.Tap ("Start Editor");

			RunningApp.EnterText ("123A");

			var editor = RunningApp.Query (q => q.Text("123"));
			Assert.That(editor.Length, Is.GreaterThanOrEqualTo(1), "The editor text should be '123'.");

			var failedEditor = RunningApp.Query (q => q.Text("123A"));
			Assert.That(failedEditor.Length, Is.EqualTo(0), "The editor text should only be '123'.");

			RunningApp.EnterText ("4");

			var editor2 = RunningApp.Query (q => q.Text("1234"));
			Assert.That(editor2.Length, Is.GreaterThanOrEqualTo(1), "The editor text should now be '1234'.");
		}
#endif
	}
}