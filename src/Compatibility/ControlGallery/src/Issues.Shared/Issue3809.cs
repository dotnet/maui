using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3809, "SetUseSafeArea is wiping out Page Padding ")]
	public class Issue3809 : TestFlyoutPage
	{
		const string _setPagePadding = "Set Page Padding";
		const string _safeAreaText = "Safe Area Enabled: ";
		const string _paddingLabel = "paddingLabel";
		const string _safeAreaAutomationId = "SafeAreaAutomation";

		Label label = null;
		protected override void Init()
		{
			label = new Label()
			{
				AutomationId = _paddingLabel
			};

			Flyout = new ContentPage() { Title = "Flyout" };
			Button button = null;
			button = new Button()
			{
				Text = $"{_safeAreaText}{true}",
				AutomationId = _safeAreaAutomationId,
				Command = new Command(() =>
				{
					bool safeArea = !Detail.On<PlatformConfiguration.iOS>().UsingSafeArea();
					Detail.On<PlatformConfiguration.iOS>().SetUseSafeArea(safeArea);
					button.Text = $"{_safeAreaText}{safeArea}";
					Device.BeginInvokeOnMainThread(() => label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}");
				})
			};

			Detail = new ContentPage()
			{
				Title = "Details",
				Content = new StackLayout()
				{
					Children =
					{
						new ListView(ListViewCachingStrategy.RecycleElement)
						{
							ItemsSource = Enumerable.Range(0,200).Select(x=> x.ToString()).ToList()
						},
						label,
						button,
						new Button()
						{
							Text = _setPagePadding,
							Command = new Command(() =>
							{
								Detail.Padding = new Thickness(25, 25, 25, 25);
								Device.BeginInvokeOnMainThread(() => label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}");
							})
						}
					}
				}
			};

			Detail.Padding = new Thickness(25, 25, 25, 25);
			Detail.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(true);
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}";
		}

#if UITEST

		void AssertSafeAreaText(string text)
		{
			var element =
				RunningApp
					.WaitForFirstElement(_safeAreaAutomationId);

			element.AssertHasText(text);
		}

		[Test]
		[Category(UITestCategories.UwpIgnore)]
		public void SafeAreaInsetsBreaksAndroidPadding()
		{
			// ensure initial paddings are honored
			AssertSafeAreaText($"{_safeAreaText}{true}");
			var element = RunningApp.WaitForFirstElement(_paddingLabel);

			bool usesSafeAreaInsets = false;
			if (element.ReadText() != "25, 25, 25, 25")
				usesSafeAreaInsets = true;

			Assert.AreNotEqual(element.ReadText(), "0, 0, 0, 0");
			if (!usesSafeAreaInsets)
				Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

			// disable Safe Area Insets
			RunningApp.Tap(_safeAreaAutomationId);
			AssertSafeAreaText($"{_safeAreaText}{false}");
			element = RunningApp.WaitForFirstElement(_paddingLabel);

			Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

			// enable Safe Area insets
			RunningApp.Tap(_safeAreaAutomationId);
			AssertSafeAreaText($"{_safeAreaText}{true}");
			element = RunningApp.WaitForFirstElement(_paddingLabel);
			Assert.AreNotEqual(element.ReadText(), "0, 0, 0, 0");

			if (!usesSafeAreaInsets)
				Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");


			// Set Padding and then disable safe area insets
			RunningApp.Tap(_setPagePadding);
			RunningApp.Tap(_safeAreaAutomationId);
			AssertSafeAreaText($"{_safeAreaText}{false}");
			element = RunningApp.WaitForFirstElement(_paddingLabel);
			Assert.AreEqual(element.ReadText(), "25, 25, 25, 25");

		}
#endif
	}
}
