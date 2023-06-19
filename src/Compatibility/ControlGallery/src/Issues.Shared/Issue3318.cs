using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3318, "[MAC] ScrollTo method is not working in Xamarin.Forms for mac platform", PlatformAffected.macOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	public class Issue3318 : TestContentPage
	{
		protected override void Init()
		{
			var stackLayout = new StackLayout();

			var list = Enumerable.Range(0, 40).Select(c => $"Item {c}").ToArray();
			var listview = new ListView { ItemsSource = list };

			var swShouldAnimate = new Switch();
			var lblShouldAnimate = new Label { Text = "Should Animate?" };

			var btnMakeVisible = new Button { Text = "Make Visible" };
			btnMakeVisible.Clicked += (s, e) =>
			{
				listview.ScrollTo(list[19], ScrollToPosition.MakeVisible, swShouldAnimate.IsToggled);
			};

			var btnCenter = new Button { Text = "Center" };
			btnCenter.Clicked += (s, e) =>
			{
				listview.ScrollTo(list[19], ScrollToPosition.Center, swShouldAnimate.IsToggled);
			};

			var btnStart = new Button { Text = "Start" };
			btnStart.Clicked += (s, e) =>
			{
				listview.ScrollTo(list[19], ScrollToPosition.Start, swShouldAnimate.IsToggled);
			};

			var btnEnd = new Button { Text = "End" };
			btnEnd.Clicked += (s, e) =>
			{
				listview.ScrollTo(list[19], ScrollToPosition.End, swShouldAnimate.IsToggled);
			};

			stackLayout.Children.Add(btnMakeVisible);
			stackLayout.Children.Add(btnCenter);
			stackLayout.Children.Add(btnStart);
			stackLayout.Children.Add(btnEnd);

			var shouldAnimateContainer = new StackLayout { Orientation = StackOrientation.Horizontal };
			shouldAnimateContainer.Children.Add(swShouldAnimate);
			shouldAnimateContainer.Children.Add(lblShouldAnimate);

			stackLayout.Children.Add(shouldAnimateContainer);
			stackLayout.Children.Add(listview);

			Content = stackLayout;
		}


#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue3318Test()
		{
			RunningApp.WaitForElement(q => q.Marked("End"));
			RunningApp.Tap(q => q.Marked("End"));
			RunningApp.WaitForElement(q => q.Marked("Item 19"));
			RunningApp.Back();
		}
#endif
	}
}
