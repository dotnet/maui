using System;
using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;

#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55912, "Tap event not always propagated to containing Grid/StackLayout",
		PlatformAffected.Android)]
	public class Bugzilla55912 : TestContentPage
	{
		const string Success = "Success";
		const string GridLabelId = "GridLabel";
		const string StackLabelId = "StackLabel";

		protected override void Init()
		{
			var layout = new Grid();

			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

			var testGrid = new Grid { BackgroundColor = Color.Red, AutomationId = "testgrid"};
			var gridLabel = new Label
			{
				AutomationId = GridLabelId,
				Text = "This is a Grid with a TapGesture",
				FontSize = 24,
				BackgroundColor = Color.Green
			};
			Grid.SetRow(testGrid, 1);
			testGrid.Children.Add(gridLabel);

			var testStack = new StackLayout { BackgroundColor = Color.Default, AutomationId = "teststack"};
			var stackLabel = new Label
			{
				AutomationId = StackLabelId,
				Text = "This StackLayout also has a TapGesture",
				FontSize = 24,
				BackgroundColor = Color.Green
			};
			Grid.SetRow(testStack, 2);
			testStack.Children.Add(stackLabel);

			layout.Children.Add(testGrid);
			layout.Children.Add(testStack);

			Content = layout;

			testGrid.GestureRecognizers.Add(new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(() =>
				{
					Debug.WriteLine($"***** TestGrid Tapped: {DateTime.Now} *****");
					layout.Children.Add(new Label { AutomationId = Success, Text = Success });
				})
			});

			testStack.GestureRecognizers.Add(new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(() =>
				{
					Debug.WriteLine($"***** TestStack Tapped: {DateTime.Now} *****");
					layout.Children.Add(new Label { AutomationId = Success, Text = Success });
				})
			});
		}

#if UITEST
		[Test]
		public void GestureBubblingInStackLayout()
		{
			RunningApp.WaitForElement(StackLabelId);
			RunningApp.Tap(StackLabelId);
			RunningApp.WaitForElement(Success);
		}

		[Test]
		public void GestureBubblingInGrid()
		{
			RunningApp.WaitForElement(GridLabelId);
			RunningApp.Tap(GridLabelId);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}