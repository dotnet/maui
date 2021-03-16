using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using AToolbarPlacement = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ToolbarPlacement;
using WindowsOS = Microsoft.Maui.Controls.PlatformConfiguration.Windows;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1898, "TabbedPage Page not watching icon changes", PlatformAffected.Android, issueTestNumber: 1)]
	public class Issue1898 : TestTabbedPage
	{
		protected override void Init() =>
			Issue1898Setup.SetupTabbedPage(this, AToolbarPlacement.Top);


#if UITEST
		[Test]
		public void TabIconsAndTitlesChange() =>
			Issue1898Setup.RunUITests(RunningApp);
#endif
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1898, "TabbedPage Page not watching icon changes", issueTestNumber: 2)]
	public partial class Issue1898_2 : TestTabbedPage
	{
		protected override void Init() =>
			Issue1898Setup.SetupTabbedPage(this, AToolbarPlacement.Bottom);

#if UITEST && __ANDROID__
		[Test]
		public void TabBottomIconsAndTitlesChange() =>
			Issue1898Setup.RunUITests(RunningApp);
#endif
	}

	[Preserve(AllMembers = true)]
	internal class Issue1898Setup
	{
#if UITEST
		internal static void RunUITests(Xamarin.UITest.IApp RunningApp)
		{
			RunningApp.WaitForElement(Title1);
			RunningApp.Tap(ChangeTitle1);
			RunningApp.Tap(ChangeIcon1);
			RunningApp.Tap(ChangeIconPage2);
			RunningApp.Tap(ChangeIcon1Null);
			RunningApp.WaitForElement(ChangeTitle1);


			RunningApp.Tap(Title2);
			RunningApp.Tap(ChangeTitle2);
			RunningApp.Tap(ChangeIcon2);
			RunningApp.Tap(ChangeIconPage1);
			RunningApp.Tap(ChangeIcon2Null);
			RunningApp.WaitForElement(ChangeTitle2);
		}
#endif

		const string Title1 = "Title 1";
		const string ChangeTitle1 = "Change Title 1";
		const string ChangeIcon1 = "Change Icon 1";
		const string ChangeIconPage2 = "Change Icon on Page 2";
		const string ChangeIcon1Null = "Null Icon 1";

		const string Title2 = "Title 2";
		const string ChangeTitle2 = "Change Title 2";
		const string ChangeIcon2 = "Change Icon 2";
		const string ChangeIconPage1 = "Change Icon on Page 1";
		const string ChangeIcon2Null = "Null Icon 2";

		internal static void SetupTabbedPage(TabbedPage tabbedPage, AToolbarPlacement placement)
		{
			ContentPage Issue1898PageOne = new ContentPage() { Title = Title1, IconImageSource = "bank.png" };
			ContentPage Issue1898PageTwo = new ContentPage() { Title = Title2, IconImageSource = "bank.png" };

			Issue1898PageOne.Content =
				new StackLayout
				{
					Margin = 20,
					Children =
					{
						new Label(){ Text = "Click through each button on each tab to make sure they do what they say they do" },
						new Button(){ Text = ChangeTitle1, Command = new Command(() => Issue1898PageOne.Title = ChangeTitle1) },
						new Button(){ Text = ChangeIcon1, Command = new Command(() => Issue1898PageOne.IconImageSource = "coffee.png")},
						new Button(){ Text = ChangeIconPage2, Command = new Command(() => Issue1898PageTwo.IconImageSource = "coffee.png")},
						new Button(){ Text = ChangeIcon1Null, Command = new Command(() => Issue1898PageOne.IconImageSource = null)},
					}
				};

			Issue1898PageTwo.Content =
				new StackLayout
				{
					Margin = 20,
					Children =
					{
						new Button(){ Text = ChangeTitle2, Command = new Command(() => Issue1898PageTwo.Title = ChangeTitle2) },
						new Button(){ Text = ChangeIcon2, Command = new Command(() => Issue1898PageTwo.IconImageSource = "bank.png")},
						new Button(){ Text = ChangeIconPage1, Command = new Command(() => Issue1898PageOne.IconImageSource = "calculator.png")},
						new Button(){ Text = ChangeIcon2Null, Command = new Command(() => Issue1898PageTwo.IconImageSource = null)},
					}
				};

			tabbedPage.Children.Add(Issue1898PageOne);
			tabbedPage.Children.Add(Issue1898PageTwo);

			tabbedPage.On<Android>().Element.UnselectedTabColor = Color.Blue;
			tabbedPage.On<Android>().Element.SelectedTabColor = Color.Green;
			tabbedPage.On<Android>().SetToolbarPlacement(placement);
			tabbedPage.On<WindowsOS>().SetHeaderIconsEnabled(true);
		}
	}
}
