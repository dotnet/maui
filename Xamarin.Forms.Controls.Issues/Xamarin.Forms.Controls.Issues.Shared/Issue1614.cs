using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1614, "iOS 11 prevents InputAccessoryView from showing in landscape mode", PlatformAffected.iOS)]
	public class Issue1614 : TestContentPage
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout();
			var picker = new Picker
			{
				AutomationId = "Picker"
			};
			var datePicker = new DatePicker
			{
				AutomationId = "DatePicker"
			};
			var timePicker = new TimePicker
			{
				AutomationId = "TimePicker"
			};

			stackLayout.Children.Add(picker);
			stackLayout.Children.Add(datePicker);
			stackLayout.Children.Add(timePicker);

			Content = stackLayout;
		}

#if UITEST && __IOS__
		protected override bool Isolate => true;

		[Test]
		public void Issue1614Test ()
		{
			RunningApp.SetOrientationPortrait();

			RunningApp.WaitForElement(x => x.Class("UITextField"));
			RunningApp.Tap(x => x.Class("UITextField").Index(0));
			CheckPickerAccessory("UIPickerView");
			RunningApp.SetOrientationLandscape();
			CheckPickerAccessory("UIPickerView");
			RunningApp.SetOrientationPortrait();
			RunningApp.DismissKeyboard();

			RunningApp.Tap(x => x.Class("UITextField").Index(1));
			CheckPickerAccessory("UIDatePicker");
			RunningApp.SetOrientationLandscape();
			CheckPickerAccessory("UIDatePicker");
			RunningApp.SetOrientationPortrait();
			RunningApp.DismissKeyboard();

			RunningApp.Tap(x => x.Class("UITextField").Index(2));
			CheckPickerAccessory("UIDatePicker");
			RunningApp.SetOrientationLandscape();
			CheckPickerAccessory("UIDatePicker");
			RunningApp.SetOrientationPortrait();
			RunningApp.DismissKeyboard();
		}

		private void CheckPickerAccessory(string className)
		{
			RunningApp.WaitForElement(x => x.Class("UIButtonLabel"));
			var buttonRect = RunningApp.Query(x => x.Class("UIButtonLabel"))[0].Rect;
			var pickerRect = RunningApp.Query(x => x.Class(className))[0].Rect;

			var buttonBottom = buttonRect.Y + buttonRect.Height;
			var pickerTop = pickerRect.Y;

			Assert.IsTrue(buttonBottom <= pickerTop);
		}
#endif
	}
}