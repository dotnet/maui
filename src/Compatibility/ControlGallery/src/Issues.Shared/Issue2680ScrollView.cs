using System;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2680, "[Enhancement] Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView", PlatformAffected.All)]
	public class Issue2680ScrollView : TestContentPage // or TestFlyoutPage, etc ... 
	{
		public bool IsScrollEnabled { get; set; } = false;

		public void ToggleButtonText()
		{
			IsScrollEnabled = !IsScrollEnabled;
			toggleButton.Text = ButtonText;
		}

		public string ButtonText => IsScrollEnabled ? ButtonDisabledCaption : ButtonEnabledCaption;

		protected override void Init()
		{
			// Initialize ui here instead of ctor 
			var longStackLayout = new StackLayout();

			toggleButton = new Button { Text = ButtonText, AutomationId = ToggleButtonMark };
			toggleButton.Clicked += ToggleButtonOnClicked;

			longStackLayout.Children.Add(toggleButton);

			longStackLayout.Children.Add(new Label
			{
				Text = "First label",
				AutomationId = FirstItemMark
			});
			Enumerable.Range(2, 50).Select(i => new Label { Text = $"Test label {i}" })
				.ToList().ForEach(label => longStackLayout.Children.Add(label));

			scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Neither,
				AutomationId = ScrollViewMark,
				Content = longStackLayout
			};

			Content = scrollView;
		}

		void ToggleButtonOnClicked(object sender, EventArgs e)
		{
			ToggleButtonText();
			scrollView.Orientation = IsScrollEnabled ? ScrollOrientation.Vertical : ScrollOrientation.Neither;
		}

		ScrollView scrollView;
		Button toggleButton;

		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		const string ButtonDisabledCaption = "Disable scroll";
		const string ButtonEnabledCaption = "Enable scroll";

#if UITEST
		[Compatibility.UITests.MovedToAppium]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void Issue2680Test_ScrollDisabled()
		{
			RunningApp.ScrollDown(ScrollViewMark);
			RunningApp.ScrollDown(ScrollViewMark);

			RunningApp.WaitForElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}

		[Compatibility.UITests.MovedToAppium]
		[Test]
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue2680Test_ScrollEnabled()
		{
			RunningApp.Tap(q => q.Button(ToggleButtonMark));

			RunningApp.ScrollDown(ScrollViewMark);
			RunningApp.ScrollDown(ScrollViewMark);

			RunningApp.WaitForNoElement(FirstItemMark, timeout: TimeSpan.FromSeconds(5));
		}
#endif
	}
}