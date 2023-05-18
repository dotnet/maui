using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8836, "[Bug] ClearButtonVisibility.Never does not take effect on UWP", PlatformAffected.UWP)]
	public class Issue8836 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();

			stack.Children.Add(new Label { Text = "Click the button to toggle the clear button visibility." });
			stack.Children.Add(new Label { Text = "The default state when this page loaded is NEVER.  The clear button should not be visible until you click toggle." });

			EntryToggle = new Entry
			{
				Text = "Clear Button: Never (toggles)",
				ClearButtonVisibility = ClearButtonVisibility.Never
			};
			stack.Children.Add(EntryToggle);

			EntryNever = new Entry
			{
				Text = "Clear Button: Never (Default Entry ClearButtonVisibility)"
			};
			stack.Children.Add(EntryNever);

			EntryAlways = new Entry
			{
				Text = "Clear Button: Always (Set before load)",
				ClearButtonVisibility = ClearButtonVisibility.WhileEditing
			};
			stack.Children.Add(EntryAlways);

			var button = new Button { Text = "Toggle Clear Button State" };
			button.Clicked += Button_Clicked;
			stack.Children.Add(button);

			Content = stack;
		}

		void Button_Clicked(object sender, System.EventArgs e)
		{
			if (EntryToggle.ClearButtonVisibility == ClearButtonVisibility.Never)
			{
				EntryToggle.ClearButtonVisibility = ClearButtonVisibility.WhileEditing;
				EntryToggle.Text = "Clear Button: While Editing (toggles)";
			}
			else
			{
				EntryToggle.ClearButtonVisibility = ClearButtonVisibility.Never;
				EntryToggle.Text = "Clear Button: Never (toggles)";
			}
			EntryToggle.Focus();
		}

		public Entry EntryToggle { get; set; }
		public Entry EntryAlways { get; set; }
		public Entry EntryNever { get; set; }

	}
}
