using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{	
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 2333, "TimePicker not shown when .Focus() is called", PlatformAffected.WinPhone)]
	public class Issue2333 : ContentPage
	{
		public Issue2333 ()
		{
			var timePicker = new TimePicker ();
			var timePickerBtn = new Button {
				Text = "Click me to call .Focus on TimePicker"
			};

			timePickerBtn.Clicked += (sender, args) => {
				timePicker.Focus ();
			};

			var timePickerBtn2 = new Button {
				Text = "Click me to call .Unfocus on TimePicker"
			};

			timePickerBtn2.Clicked += (sender, args) => {
				timePicker.Unfocus ();
			};

			Content = new StackLayout {
				Children = {
					timePickerBtn,
					timePickerBtn2,
					timePicker,
				}
			};
		}
	}
}
