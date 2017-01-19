using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51503, "NullReferenceException on VisualElement Finalize", PlatformAffected.All)]
	public class Bugzilla51503 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new RootPage());
		}

		[Preserve(AllMembers = true)]
		class RootPage : ContentPage
		{
			public RootPage()
			{
				Button button = new Button
				{
					AutomationId = "Button",
					Text = "Open"					
				};

				button.Clicked += Button_Clicked;

				Content = button;
			}

			async void Button_Clicked(object sender, EventArgs e)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();

				await Navigation.PushAsync(new ChildPage());
			}
		}

		[Preserve(AllMembers = true)]
		class ChildPage : ContentPage
		{
			public ChildPage()
			{
				Content = new Label
				{
					AutomationId = "VisualElement",
					Text = "Navigate 3 times to this page",
					Triggers = 
					{
						new EventTrigger()
					}
				};
			}
		}

#if UITEST
[Test]
		public void Issue51503Test()
		{
			for (int i = 0; i < 3; i++)
			{
				RunningApp.WaitForElement(q => q.Marked("Button"));
				
				RunningApp.Tap(q => q.Marked("Button"));

				RunningApp.WaitForElement(q => q.Marked("VisualElement"));

				RunningApp.Back();
			}
		}
#endif
	}
}