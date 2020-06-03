using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8836, "[Bug] ClearButtonVisibility.Never does not take effect on UWP", PlatformAffected.UWP)]
	public class Issue8836 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();

			stack.Children.Add(new Label {Text = "Click the button to toggle the clear button visibility."});

			Entry = new Entry
			{
				Text = "Clear Button: While Editing",
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing
			};
			stack.Children.Add(Entry);

			var button = new Button { Text = "Toggle Clear Button State" };
			button.Clicked += Button_Clicked;
			stack.Children.Add(button);

			Content = stack;
		}

		private void Button_Clicked(object sender, System.EventArgs e)
		{
			if (Entry.ClearButtonVisibility == ClearButtonVisibility.Never)
			{
				Entry.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
				Entry.Text = "Clear Button: While Editing";
			}
			else
			{
				Entry.ClearButtonVisibility = ClearButtonVisibility.Never;
				Entry.Text = "Clear Button: Never";
			}
			Entry.Focus();
		}

		public Entry Entry { get; set; }

	}
}
