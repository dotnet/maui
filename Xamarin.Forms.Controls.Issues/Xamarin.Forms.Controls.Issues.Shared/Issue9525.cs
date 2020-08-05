using System.Collections.Generic;
using System.Reflection;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9525, "MediaElement Disposing exception when MainPage is changed on iOS", PlatformAffected.iOS)]
	public class Issue9525 : TestNavigationPage
	{
		protected override void Init()
		{
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "MediaElement_Experimental" });

			PushAsync(CreateRoot());
		}
		private ContentPage CreateRoot()
		{ 
			var button = new Button
			{
				AutomationId = "Issue9525Button",
				Text = "Go to new page",
			};
			button.Clicked += Button_Clicked;
			return new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						new MediaElement
						{
							AutomationId = "Issue9525MediaElement",
							Source = "https://sec.ch9.ms/ch9/80a3/6563611f-6a39-44fa-a768-1a58bdd080a3/HotRestart.mp4",
							HeightRequest=200,
						},
						button
					}
				}
			};
			
		}
		private void Button_Clicked(object sender, System.EventArgs e)
		{
			Navigation.InsertPageBefore(CreateRoot(), CurrentPage);
			Navigation.RemovePage(CurrentPage);
		}


#if UITEST
		[Test]
		public void Issue9525Test()
		{
			//Will be exeption if fail.
			RunningApp.Screenshot("I am at Issue9525");
			for (var i = 0; i < 10; i++)
			{
				RunningApp.WaitForElement(q => q.Marked("Issue9525Button"));
				RunningApp.Screenshot("I see the Button");
				RunningApp.Tap("Issue9525Button");
			}
		}
#endif
	}
}
