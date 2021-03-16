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
	[Issue(IssueTracker.Github, 6262, "[Bug] Button in Grid gets wrong z-index",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Layout)]
#endif
	public class Issue6262 : TestContentPage
	{
		protected override void Init()
		{
			Grid theGrid = null;
			theGrid = new Grid() { VerticalOptions = LayoutOptions.FillAndExpand };
			SetupGrid(theGrid);

			Content = new StackLayout()
			{
				Children =
				{
					theGrid,
					new Button()
					{
						Text = "Click this and see if test succeeds. If you don't see failure text then test has passed",
						AutomationId = "RetryTest",
						Command = new Command(() => SetupGrid(theGrid))
					}
				}
			};

		}

		void SetupGrid(Grid theGrid)
		{
			if (theGrid.Children.Count > 0)
				theGrid.Children.Clear();

			theGrid.Children.Add(
				new Button()
				{
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					Text = "If you can see this the test has failed",
					AutomationId = "ClickMe",
					Command = new Command(() =>
					{
						theGrid.Children.Clear();
						theGrid.Children.Add(new Label() { AutomationId = "Fail", Text = "Test Failed" });
					})
				});

			theGrid.Children.Add(
				new Image()
				{
					Source = "coffee.png",
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill,
					AutomationId = "ClickMe",
					BackgroundColor = Color.Green
				});
		}

#if UITEST
		[Test]
		public void ImageShouldLayoutOnTopOfButton()
		{
			RunningApp.WaitForElement("ClickMe");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.WaitForNoElement("Fail");
			RunningApp.Tap("RetryTest");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.WaitForNoElement("Fail");
		}
#endif
	}
}
