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
	[Category(UITestCategories.Button)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7875, "Button size changes when setting Accessibility properties", PlatformAffected.Android)]
	public class Issue7875 : TestContentPage
	{
		public Issue7875()
		{
			Title = "Issue 7875";

			var layout = new Grid();

			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If the buttons below have the same size, the test has passed."
			};
			layout.Children.Add(instructions, 0, 0);

			var button = new Button
			{
				BackgroundColor = Color.Gray,
				HorizontalOptions = LayoutOptions.Center,
				ImageSource = "calculator.png",
				Text = "Text"
			};
			layout.Children.Add(button, 0, 1);

			var accesibilityButton = new Button
			{
				BackgroundColor = Color.Gray,
				HorizontalOptions = LayoutOptions.Center,
				ImageSource = "calculator.png",
				Text = "Text"
			};
			accesibilityButton.SetValue(AutomationProperties.NameProperty, "AccesibilityButton");
			accesibilityButton.SetValue(AutomationProperties.HelpTextProperty, "Help Large Text");
			layout.Children.Add(accesibilityButton, 0, 2);

			Content = layout;
		}

		protected override void Init()
		{

		}
	}
}