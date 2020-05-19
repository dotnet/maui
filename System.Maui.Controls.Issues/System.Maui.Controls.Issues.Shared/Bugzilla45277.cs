using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45277, "[WinRT/UWP] Entry with IsPassword = true does not allow selection of characters", PlatformAffected.WinRT)]
	public class Bugzilla45277 : TestContentPage
	{
		protected override void Init()
		{
			var passwordTextLabel = new Label
			{
				Text = "[No Password Text Yet]"
			};
			var passwordEntry = new Entry
			{
				IsPassword = true,
				Placeholder = "Enter password"
			};
			passwordEntry.Completed += (sender, args) => DisplayAlert("Enter pressed", "OK", "Cancel");
			passwordEntry.TextChanged += (sender, args) =>
			{
				passwordTextLabel.Text = passwordEntry.Text;
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The below label should allow Paste and Select All commands; however, copying via keyboard should not work."
					},
					passwordEntry,
					new Entry
					{
						Placeholder = "Entry for easily testing your paste functionality"
					},
					passwordTextLabel
				}
			};
		}
	}
}