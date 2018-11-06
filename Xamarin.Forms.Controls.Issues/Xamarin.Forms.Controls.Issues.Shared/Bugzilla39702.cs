using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39702, "Cannot enter text when Entry is focus()'d from an editor completed event")]
	public class Bugzilla39702 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			Title = "focus test";
			var editor = new Editor();
			var entry = new Entry();

			editor.Unfocused += (object sender, FocusEventArgs e) => entry.Focus();

			Content = new StackLayout
			{
				Children =
				{
					editor,
					entry
				}
			};
		}
	}
}
