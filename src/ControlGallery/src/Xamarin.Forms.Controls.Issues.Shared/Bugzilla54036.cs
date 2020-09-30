using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 54036, "[UWP] MasterPage - Bad Rendering", PlatformAffected.UWP)]
	public class Bugzilla54036 : TestFlyoutPage
	{
		class MasterPage : ContentPage
		{
			public MasterPage()
			{
				Title = "Flyout";
				var grid = new Grid
				{
					RowDefinitions =
					{
						new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
						new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
						new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
						new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
					}
				};

				var label1 = new Label { Text = "On UWP, the very last Label will be visible until the Flyout is hidden and presented again." };
				var label2 = new Label { Text = "If you do not see a Label that says Success at the bottom of the Flyout" };
				var label3 = new Label { Text = "then this test has failed." };
				var label4 = new Label { Text = "Success" };

				grid.AddChild(label1, 0, 0);
				grid.AddChild(label2, 0, 1);
				grid.AddChild(label3, 0, 2);
				grid.AddChild(label4, 0, 3);

				Content = grid;
			}
		}

		protected override void Init()
		{
			Flyout = new MasterPage();
			Detail = new ContentPage();

			IsPresented = true;

			Device.StartTimer(TimeSpan.FromMilliseconds(500), () => { IsPresented = false; return false; });
			Device.StartTimer(TimeSpan.FromMilliseconds(1000), () => { IsPresented = true; return false; });
		}
	}
}