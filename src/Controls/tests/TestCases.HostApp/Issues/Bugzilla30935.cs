using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 30935, "NullReferenceException in ViewRenderer<TView, TNativeView> (Microsoft.Maui.Controls.Platform.Android)")]
	public class Bugzilla30935 : TestContentPage
	{
		Entry _entry;
		protected override void Init()
		{
			_entry = new Entry { AutomationId = "entry" };
			// Initialize ui here instead of ctor
			Content = new StackLayout
			{
				Children = { new Label {
						AutomationId = "IssuePageLabel",
						Text = "See if I'm here"
					},_entry
				}
			};
		}

		protected override void OnAppearing()
		{
			_entry.Focus();
			Content = null;
			base.OnAppearing();
		}
	}
}