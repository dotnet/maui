using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3342, "[Android] BoxView BackgroundColor not working on 3.2.0-pre1", PlatformAffected.Android)]
	public class Issue3342 : TestContentPage 
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "You should see a green BoxView. You should not see the text that says FAIL."
			};

			var hiddenLabel = new Label
			{
				Text = "FAIL"
			};

			var target = new BoxView
			{
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Color.Green,
				CornerRadius = new CornerRadius(3)
			};

			var grid = new Grid
			{
				ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
				RowDefinitions = {  new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
									new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }}
			};

			grid.Children.Add(instructions);
			grid.Children.Add(hiddenLabel, 0, 1);
			grid.Children.Add(target, 0, 1);

			Content = grid;
		}

#if UITEST
		[Test]
		public void Issue3342Test ()
		{
			RunningApp.Screenshot ("I am at Issue 3342");
			//RunningApp.WaitForNoElement (q => q.Marked ("FAIL"));
			RunningApp.Screenshot ("I see the green box");
		}
#endif
	}
}
