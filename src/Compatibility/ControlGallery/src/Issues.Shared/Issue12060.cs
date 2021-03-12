using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12060, "Bug] DragGestureRecognizer shows 'Copy' tag when dragging in UWP",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github10000)]
	[NUnit.Framework.Category(UITestCategories.DragAndDrop)]
#endif
	public class Issue12060 : TestContentPage
	{
		protected override void Init()
		{
			Label testResult = new Label()
			{
				AutomationId = "Result",
				Text = "Running"
			};

			var drag = new DragGestureRecognizer();

			drag.DropCompleted += (_, __) =>
			{
				if (testResult.Text == "Running")
					testResult.Text = "Success";
			};

			BoxView boxView = new BoxView()
			{
				HeightRequest = 200,
				WidthRequest = 1000,
				BackgroundColor = Color.Purple,
				GestureRecognizers =
				{
					drag
				},
				AutomationId = "DragBox"
			};

			var dropGestureRecognizer = new DropGestureRecognizer();

			dropGestureRecognizer.DragOver += (_, args) =>
			{
				args.AcceptedOperation = DataPackageOperation.None;
			};

			dropGestureRecognizer.Drop += (_, args) =>
			{
				testResult.Text = "Fail";
			};

			BoxView boxView2 = new BoxView()
			{
				HeightRequest = 200,
				WidthRequest = 1000,
				BackgroundColor = Color.Pink,
				AutomationId = "DropBox"
			};

			boxView2.GestureRecognizers.Add(dropGestureRecognizer);

			Content = new StackLayout()
			{
				Children =
				{
					boxView,
					boxView2,
					new Label()
					{
						Text = "Drag the top box to the bottom one. The drop operation for the bottom box should be disabled.",
						AutomationId = "TestLoaded"
					},
					testResult
				}
			};
		}

#if UITEST
		[Test]
		public void AcceptedOperationNoneDisablesDropOperation()
		{
			RunningApp.WaitForElement("TestLoaded");
			RunningApp.DragAndDrop("DragBox", "DropBox");
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
