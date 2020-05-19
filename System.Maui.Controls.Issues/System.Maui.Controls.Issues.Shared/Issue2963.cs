using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2963, "Disabling Editor in iOS does not disable entry of text")]
	public class Issue2963 : TestContentPage
	{
		readonly string _editorId = "DisabledEditor";
		readonly string _focusedLabelId = "FocusedLabel";

		protected override void Init ()
		{
			
			var disabledEditor = new Editor {
				AutomationId = _editorId,
				Text = "You should not be able to edit me",
				IsEnabled = false
			};

			BindingContext = disabledEditor;
			var focusedLabel = new Label {
				AutomationId = _focusedLabelId
			};
			focusedLabel.SetBinding (Label.TextProperty, "IsFocused");

			Content = new StackLayout {
				Children = {
					disabledEditor,
					focusedLabel,
				}
			};
		}

#if UITEST
		[Test]
		public void Issue2963Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2963");
			RunningApp.Tap (q => q.Marked (_editorId));
			Assert.AreEqual ("False", RunningApp.Query (q => q.Marked (_focusedLabelId))[0].Text);
			RunningApp.Screenshot ("Label should still be false");
		}
#endif
	}
}
