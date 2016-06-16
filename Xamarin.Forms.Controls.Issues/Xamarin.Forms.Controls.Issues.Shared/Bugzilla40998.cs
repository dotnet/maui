using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40998, "[UWP] Pressing escape with an awaited DisplayActionSheet doesn't return a result", PlatformAffected.WinRT)]
	public class Bugzilla40998 : TestContentPage
	{
		protected override void Init()
		{
			var resultLabel = new Label
			{
				Text = "ActionSheet Result - use the ActionSheet to show the result"
			};
			Content = new StackLayout
			{
				Children =
				{
					resultLabel,
					new Button
					{
						Text = "Click to display ActionSheet",
						Command = new Command(async () =>
						{
							var result = await DisplayActionSheet("Test ActionSheet", "Cancel", "Destroy", new string[] { "Test Button" });
							resultLabel.Text = result;
						})
					}
				}
			};
		}
	}
}
