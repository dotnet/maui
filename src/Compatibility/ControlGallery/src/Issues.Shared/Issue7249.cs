using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7249, "(Android) Wrong color on Slider", PlatformAffected.Android)]
	public class Issue7249 : TestContentPage
	{
		public Issue7249()
		{
			Title = "Issue 7249";
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Padding = new Thickness(12)
			};

			var instructions = new Label
			{
				Text = "Toggle the first Switch and verify that the color of the Thumb is equal to the Thumb color of the second Switch."
			};

			var switch1 = new Switch
			{
				HorizontalOptions = LayoutOptions.Start,
				IsToggled = false
			};

			var switch2 = new Switch
			{
				HorizontalOptions = LayoutOptions.Start,
				IsToggled = true
			};

			var instructions2 = new Label
			{
				Text = "The Switch below uses a Custom Renderer to validate that can override colors using a Custom Renderer."
			};

			var customSwitch = new Issue7249Switch
			{
				SwitchOffColor = Color.Red,
				SwitchOnColor = Color.Green,
				SwitchThumbColor = Color.Yellow,
				HorizontalOptions = LayoutOptions.Start,
				IsToggled = false
			};

			layout.Children.Add(instructions);
			layout.Children.Add(switch1);
			layout.Children.Add(switch2);
			layout.Children.Add(instructions2);
			layout.Children.Add(customSwitch);

			Content = layout;
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue7249Switch : Switch
	{
		public static readonly BindableProperty SwitchOffColorProperty =
		  BindableProperty.Create(nameof(SwitchOffColor),
			  typeof(Color), typeof(CustomSwitch),
			  Color.Default);

		public Color SwitchOffColor
		{
			get { return (Color)GetValue(SwitchOffColorProperty); }
			set { SetValue(SwitchOffColorProperty, value); }
		}

		public static readonly BindableProperty SwitchOnColorProperty =
		  BindableProperty.Create(nameof(SwitchOnColor),
			  typeof(Color), typeof(CustomSwitch),
			  Color.Default);

		public Color SwitchOnColor
		{
			get { return (Color)GetValue(SwitchOnColorProperty); }
			set { SetValue(SwitchOnColorProperty, value); }
		}

		public static readonly BindableProperty SwitchThumbColorProperty =
		  BindableProperty.Create(nameof(SwitchThumbColor),
			  typeof(Color), typeof(CustomSwitch),
			  Color.Default);

		public Color SwitchThumbColor
		{
			get { return (Color)GetValue(SwitchThumbColorProperty); }
			set { SetValue(SwitchThumbColorProperty, value); }
		}
	}
}