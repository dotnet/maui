using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6945, "[iOS] Wrong anchor behavior when setting HeightRequest ",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue6946 : TestContentPage
	{
		const string ClickMeId = "ClickMeAutomationId";
		const string BoxViewId = "BoxViewAutomationId";

		protected override void Init()
		{
			var boxView = new BoxView()
			{
				AnchorX = 0,
				AnchorY = 0,
				HeightRequest = 150,
				WidthRequest = 150,
				Color = Colors.Red,
				TranslationX = 101,
				TranslationY = 201,
				AutomationId = BoxViewId
			};

			Button button = new Button()
			{
				Text = "Click Me. Box X/Y position should not change",
				TranslationY = 171,
				TranslationX = 0,
				Command = new Command(() =>
				{
					boxView.HeightRequest = 160;
				}),
				AutomationId = ClickMeId
			};

			Content =
				new AbsoluteLayout()
				{
					Children =
					{
						boxView,
						button
					}
				};
		}


#if UITEST
		[Test]
		public void WrongTranslationBehaviorWhenChangingHeightRequestAndSettingAnchor()
		{
			var rect = RunningApp.WaitForElement(BoxViewId)[0].Rect;
			RunningApp.Tap(ClickMeId);
			var rect2 = RunningApp.WaitForElement(BoxViewId)[0].Rect;

			Assert.AreEqual(rect.X, rect2.X);
			Assert.AreEqual(rect.Y, rect2.Y);
		}
#endif
	}
}
