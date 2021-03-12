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
	[Issue(IssueTracker.Github, 6260, "[Android] infinite layout loop",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Button)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	public class Issue6260 : TestContentPage
	{
		const string text = "If this number keeps increasing test has failed: ";
		static string success = text + "0";

		protected override void Init()
		{
			int measurecount = 0;

			var button = new Button()
			{
				Text = "Click me",
				BackgroundColor = Color.Green,

			};

			var label = new Label()
			{
				Text = success
			};

			this.Appearing += (_, __) =>
			{
				button.ImageSource = "coffee.png";
				Device.BeginInvokeOnMainThread(() =>
				{
					button.MeasureInvalidated += (___, ____) =>
					{
						measurecount++;
						label.Text = text + measurecount.ToString();
					};
				});
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Welcome to Xamarin.Forms!",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					},
					new Entry(),
					button,
					label
				}
			};
		}

#if UITEST
		[Test]
		public void ButtonImageInfiniteLayout()
		{
			RunningApp.WaitForElement(success);
		}
#endif
	}
}
