using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 48236, "[WinRT/UWP] BackgroundColor for Stepper behaves differently compared to iOS to Android", PlatformAffected.WinRT)]
	public class Bugzilla48236 : TestContentPage
	{
		protected override void Init()
		{
			var stepper = new Stepper
			{
				BackgroundColor = Color.Green,
				Minimum = 0,
				Maximum = 10
			};

			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "If the Stepper's background color extends the width of the page, then this test has failed."
					},
					stepper,
					new Button
					{
						BackgroundColor = Color.Aqua,
						Text = "Change Stepper Color to Yellow",
						Command = new Command(() =>
						{
							stepper.BackgroundColor = Color.Yellow;
						})
					}
				}
			};
		}
	}
}