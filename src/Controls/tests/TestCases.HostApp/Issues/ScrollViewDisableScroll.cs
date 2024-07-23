using System;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// Issue2680Test_ScrollEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\Issue2680ScrollView.cs)
	// Issue2680Test_ScrollDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\Issue2680ScrollView.cs)
	[Issue(IssueTracker.None, 2680, "Add VerticalScrollMode/HorizontalScrollMode to ListView and ScrollView", PlatformAffected.All)]
	public class ScrollViewDisableScroll : ContentPage
	{
		const string ScrollViewMark = "ScrollView";
		const string FirstItemMark = "FirstItem";
		const string ToggleButtonMark = "ToggleButton";

		const string ButtonDisabledCaption = "Disable scroll";
		const string ButtonEnabledCaption = "Enable scroll";

		readonly ScrollView _scrollView;
		readonly Button _toggleButton;

		public bool IsScrollEnabled { get; set; } = false;

		public void ToggleButtonText()
		{
			IsScrollEnabled = !IsScrollEnabled;
			_toggleButton.Text = ButtonText;
		}

		public string ButtonText => IsScrollEnabled ? ButtonDisabledCaption : ButtonEnabledCaption;

		public ScrollViewDisableScroll()
		{
			Title = "ScrollView DisableScroll";

			// Initialize ui here instead of ctor 
			var longStackLayout = new StackLayout();

			_toggleButton = new Button { Text = ButtonText, AutomationId = ToggleButtonMark };
			_toggleButton.Clicked += ToggleButtonOnClicked;

			longStackLayout.Children.Add(_toggleButton);

			longStackLayout.Children.Add(new Label
			{
				Text = "First label",
				AutomationId = FirstItemMark
			});
			Enumerable.Range(2, 50).Select(i => new Label { Text = $"Test label {i}" })
				.ToList().ForEach(label => longStackLayout.Children.Add(label));

			_scrollView = new ScrollView
			{
				Orientation = ScrollOrientation.Neither,
				AutomationId = ScrollViewMark,
				Content = longStackLayout
			};

			Content = _scrollView;
		}

		void ToggleButtonOnClicked(object sender, EventArgs e)
		{
			ToggleButtonText();
			_scrollView.Orientation = IsScrollEnabled ? ScrollOrientation.Vertical : ScrollOrientation.Neither;
		}
	}
}