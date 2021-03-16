using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1909, "Xamarin.forms 2.5.0.280555 and android circle button issue", PlatformAffected.Android)]
	public class Issue1909 : TestContentPage
	{
		public class FlatButton : Button { }
		protected override void Init()
		{
			Button button = new Button
			{
				BackgroundColor = Color.Red,
				CornerRadius = 32,
				BorderWidth = 0,
				FontSize = 36,
				HeightRequest = 64,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Color.White,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 64,
				AutomationId = "TestReady"
			};

			button.On<Android>().SetUseDefaultPadding(true).SetUseDefaultShadow(true);

			FlatButton flatButton = new FlatButton
			{
				BackgroundColor = Color.Red,
				CornerRadius = 32,
				BorderWidth = 0,
				FontSize = 36,
				HeightRequest = 64,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Color.White,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 64
			};

			Content = new StackLayout
			{
				Children = {
					new Label{ Text = "The following buttons should be perfectly round. The bottom button should be larger and should not have a shadow." },
					button,
					flatButton
				}
			};
		}
#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue1909Test()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("I am at Issue 1909");
		}
#endif
	}
}