using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 37290, "[WinRT/UWP] Setting ActivityIndicator.IsRunning=\"false\" shows the ActivityIndicator", PlatformAffected.WinRT)]
	public class Bugzilla37290 : TestContentPage
	{
		protected override void Init()
		{
			var activityIndicator = new ActivityIndicator
			{
				IsRunning = false,
				Opacity = 0.4
			};
			var opacityStepper = new Stepper
			{
				Minimum = 0.1,
				Maximum = 1.0,
				Increment = .1,
				Value = 0.4
			};
			var stepperValue = new Label
			{
				Text = "Current Value: " + opacityStepper.Value.ToString()
			};
			opacityStepper.ValueChanged += (s, e) =>
			{
				activityIndicator.Opacity = opacityStepper.Value;
				stepperValue.Text = "Current Value: " + opacityStepper.Value.ToString();
			};
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The activity indicator below should not be initially visible. You can also use the stepper to change its Opacity value."
					},
					activityIndicator,
					new Button
					{
						Text = "Click to toggle IsRunning on the ActivityIndicator",
						Command = new Command(() => activityIndicator.IsRunning = !activityIndicator.IsRunning)
					},
					new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children =
						{
							opacityStepper,
							stepperValue
						}
					}
				}
			};
		}
	}
}