using System.Threading.Tasks;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Entry)]
	[Category(UITestCategories.Editor)]
	[Category(UITestCategories.Focus)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39702, "Cannot enter text when Entry is focus()'d from an editor completed event")]
	public class Bugzilla39702 : TestContentPage 
	{
		const string TheEntry = "TheEntry";
		const string Success = "Success";

		protected override async void Init()
		{
			Title = "focus test";
			var editor = new Editor();
			var entry = new Entry { AutomationId = TheEntry };
			var result = new Label();

			var instructions = new Label
			{
				Text = "Wait 4 seconds; the Entry (third control from the top) should be focused, and the keyboard" 
						+ " should be visible. Typing on the keyboard should enter text into the Entry."
						+ " If the typing does not enter text into the Entry, this test has failed."
			};

			editor.Unfocused += (sender, e) => entry.Focus();
			entry.TextChanged += (sender, args) => result.Text = args.NewTextValue;

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					editor,
					entry,
					result
				}
			};

			await Task.Delay(2000);
			editor.Focus();
			await Task.Delay(1000);
			editor.Unfocus();
		}

#if UITEST
		[Test]
		public async Task ControlCanBeFocusedByUnfocusedEvent()
		{
			RunningApp.WaitForElement(TheEntry);
			await Task.Delay(4000);
			RunningApp.EnterText(Success); // Should be typing into the Entry at this point
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
