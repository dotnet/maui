using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 52266, "[WinRT/UWP] Picker.Focus() does not open the dropdown", PlatformAffected.WinRT)]
	public class Bugzilla52266 : TestContentPage
	{
		protected override void Init()
		{
			var picker = new Picker
			{
				ItemsSource = new string[] { "A", "B", "C" }
			};
			Content = new StackLayout
			{
				Children =
				{
					picker,
					new Button
					{
						Text = "Click to focus the picker",
						Command = new Command(() =>
						{
							picker.Focus();
						})
					}
				}
			};
		}
	}
}