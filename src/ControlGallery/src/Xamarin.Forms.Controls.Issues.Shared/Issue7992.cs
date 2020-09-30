using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7992, "Datepicker is not opened when we call Datepicker.Focus() in UWP", PlatformAffected.UWP)]

	public class Issue7992 : TestContentPage
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout();
			Content = stackLayout;

			stackLayout.Children.Add(new Label
			{
				Text = "Label to keep picker from getting initial focus"
			});

			var datePicker = new DatePicker();
			stackLayout.Children.Add(datePicker);

			var buttonFocus = new Button
			{
				Text = "Calls Focus() on the date picker, which should open a date picker flyout."
			};
			buttonFocus.Clicked += (s, e) =>
			{
				datePicker.IsVisible = true;
				datePicker.Focus();
			};
			stackLayout.Children.Add(buttonFocus);

			var buttonNotVisible = new Button
			{
				Text = "Makes the picker not visible and calls Focus(), which should open a full screen picker flyout."
			};
			buttonNotVisible.Clicked += (s, e) =>
			{
				datePicker.IsVisible = false;
				datePicker.Focus();
			};
			stackLayout.Children.Add(buttonNotVisible);

		}

	}
}
