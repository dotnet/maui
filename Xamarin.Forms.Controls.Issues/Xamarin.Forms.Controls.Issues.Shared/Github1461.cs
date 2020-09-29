using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1461, "Cannot change TimePicker format", PlatformAffected.Android)]
	public class Github1461 : TestContentPage // or TestFlyoutPage, etc ...
	{
		Button _button = new Button();
		TimePicker _timePicker = new TimePicker();

		protected override void Init()
		{
			var label = new Label
			{
				Text = "On start, time should be in military format. Clicking the button changes the format and the time according to button text. Also observe that time picker dialog's time format is aligned with the time picker text format."
			};

			_timePicker.Time = TimeSpan.FromHours(17).Add(TimeSpan.FromMinutes(32));
			_timePicker.Format = "HH:mm";

			_button.Text = "Show standard time";
			_button.Clicked += _button_Clicked;

			Content = new StackLayout
			{
				Spacing = 15,
				Children = { label, _button, _timePicker }
			};
		}

		private void _button_Clicked(object sender, EventArgs e)
		{
			if (_timePicker.Format == "HH:mm")
			{
				_timePicker.Format = "hh:mm";
				_button.Text = "Show military time";
			}
			else
			{
				_timePicker.Format = "HH:mm";
				_button.Text = "Show standard time";
			}
		}
	}
}