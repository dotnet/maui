using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3872, "TabStop additions cause a non interactive TabStop on Time Picker...", PlatformAffected.UWP)]
	public class GitHub3872 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children = {
					new Entry(),
					new TimePicker(),
					new DatePicker(),
					new SearchBar(),
					new Stepper(),
					new Entry()
				}
			};
		}
	}
}