using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest.Queries;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7823, "[Bug] Frame corner radius.", PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	public class Issue7823 : TestContentPage
	{
		const string GetClipToOutline = "getClipToOutline";
		const string GetClipChildren = "getClipChildren";
		const string GetClipBounds = "getClipBounds";
		const string SetClipBounds = "SetClipBounds";
		const string SecondaryFrame = "SecondaryFrame";
		const string RootFrame = "Root Frame";
		const string BoxView = "Box View";

		protected override void Init()
		{
			var frameClippedToBounds = new Frame
			{
				IsClippedToBounds = true,
				AutomationId = SecondaryFrame,
				CornerRadius = 10,
				BackgroundColor = Colors.Blue,
				Padding = 0,
				Content = new BoxView
				{
					AutomationId = BoxView,
					BackgroundColor = Colors.Green,
					WidthRequest = 100,
					HeightRequest = 100
				}
			};

			Content = new StackLayout()
			{
				Children =
				{
					new ApiLabel(),
					new Frame
					{
						AutomationId = RootFrame,
						CornerRadius = 5,
						BackgroundColor = Colors.Red,
						Padding = 10,
						Content = frameClippedToBounds
					},
					new Button
					{
						AutomationId = SetClipBounds,
						Text = "Manually set Frame.IsClippedToBounds = false",
						Command = new Command(()=>
						{
							frameClippedToBounds.IsClippedToBounds = false;
							frameClippedToBounds.CornerRadius = 11;
						})
					}
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		[UiTest(typeof(Frame))]
		public void Issue7823TestIsClippedIssue()
		{
			RunningApp.WaitForElement(RootFrame);
			AssertIsClipped(true);
			RunningApp.Tap(SetClipBounds);
			AssertIsClipped(false);
		}

		void AssertIsClipped(bool expected)
		{
			if (RunningApp.IsApiHigherThan(21))
			{
				var cliptoOutlineValue = RunningApp.InvokeFromElement<bool>(SecondaryFrame, GetClipToOutline)[0];
				Assert.AreEqual(expected, cliptoOutlineValue);
			}
			else if (RunningApp.IsApiHigherThan(19))
			{
				var clipBounds = RunningApp.InvokeFromElement<object>(SecondaryFrame, GetClipBounds)[0];
				if (expected)
					Assert.IsNotNull(clipBounds);
				else
					Assert.IsNull(clipBounds);
			}
			else
			{
				var clipChildrenValue = RunningApp.InvokeFromElement<bool>(SecondaryFrame, GetClipChildren)[0];
				Assert.AreEqual(expected, clipChildrenValue);
			}
		}
#endif
	}
}
