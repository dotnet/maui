using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4961, "TimePicker does not remeasure its size when picking a new time that is wider than the previously selected value",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue4961 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "When changing the Time of the TimePicker the control should be remeasured and its size may change. " +
							"Set the time to 12:50 AM and check that the text does not get out of the screen."
					},
					new TimePicker
					{
						Time = TimeSpan.FromHours(1),
						HorizontalOptions = LayoutOptions.EndAndExpand,
						VerticalOptions = LayoutOptions.Start
					}
				}
			};
		}
	}
}