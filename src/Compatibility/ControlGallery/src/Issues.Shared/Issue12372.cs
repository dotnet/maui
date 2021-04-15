using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
    [Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12372, "[Bug] XF 4.8 breaks custom renderer (Button) background color on iOS", PlatformAffected.iOS)]
	public class Issue12372 : TestContentPage
	{
		public Issue12372()
		{

		}

		protected override void Init()
		{
			Title = "Issue 12372";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "If you can see the background color of the custom Button below, the test has passed."
			};

			var button = new Issue12372Button
			{
				NymblDefaultColor = Colors.Blue,
				NymblPressedColor = Colors.Red,
				NymblTextColor = Colors.White,
				Text = "Issue12372"
			};

			layout.Children.Add(instructions);
			layout.Children.Add(button);

			Content = layout;
		}
	}

	public class Issue12372Button : Button
	{
		public readonly static BindableProperty NymblDefaultColorProperty = BindableProperty.Create(
			nameof(NymblDefaultColor),
			typeof(Color),
			typeof(Issue12372Button),
			Colors.Blue);

		public Color NymblDefaultColor
		{
			get => (Color)GetValue(NymblDefaultColorProperty);
			set => SetValue(NymblDefaultColorProperty, value);
		}

		public readonly static BindableProperty NymblPressedColorProperty = BindableProperty.Create(
			nameof(NymblPressedColor),
			typeof(Color),
			typeof(Issue12372Button),
			Colors.Blue);

		public Color NymblPressedColor
		{
			get => (Color)GetValue(NymblPressedColorProperty);
			set => SetValue(NymblPressedColorProperty, value);
		}

		public readonly static BindableProperty NymblDisabledColorProperty = BindableProperty.Create(
			nameof(NymblDisabledColor),
			typeof(Color),
			typeof(Issue12372Button),
			Colors.Blue);

		public Color NymblDisabledColor
		{
			get => (Color)GetValue(NymblDisabledColorProperty);
			set => SetValue(NymblDisabledColorProperty, value);
		}

		public readonly static BindableProperty NymblDisabledTextColorProperty = BindableProperty.Create(
			 nameof(NymblDisabledTextColor),
			 typeof(Color),
			 typeof(Issue12372Button),
			 Colors.LightSlateGray);

		public Color NymblDisabledTextColor
		{
			get => (Color)GetValue(NymblDisabledTextColorProperty);
			set => SetValue(NymblDisabledTextColorProperty, value);
		}

		public readonly static BindableProperty NymblBorderColorProperty = BindableProperty.Create(
			nameof(NymblBorderColor),
			typeof(Color),
			typeof(Issue12372Button),
			Colors.White);

		public Color NymblBorderColor
		{
			get => (Color)GetValue(NymblBorderColorProperty);
			set => SetValue(NymblBorderColorProperty, value);
		}

		public readonly static BindableProperty NymblBorderWidthProperty = BindableProperty.Create(
			nameof(NymblBorderWidth),
			typeof(int),
			typeof(Issue12372Button));

		public int NymblBorderWidth
		{
			get => (int)GetValue(NymblBorderWidthProperty);
			set => SetValue(NymblBorderWidthProperty, value);
		}

		public readonly static BindableProperty NymblTextColorProperty = BindableProperty.Create(
			nameof(NymblTextColor),
			typeof(Color),
			typeof(Issue12372Button),
			Colors.White);

		public Color NymblTextColor
		{
			get => (Color)GetValue(NymblTextColorProperty);
			set => SetValue(NymblTextColorProperty, value);
		}

		public readonly static BindableProperty NymblTextAllCapsProperty = BindableProperty.Create(
			nameof(NymblTextAllCaps),
			typeof(bool),
			typeof(Issue12372Button));

		public bool NymblTextAllCaps
		{
			get => (bool)GetValue(NymblTextAllCapsProperty);
			set => SetValue(NymblTextAllCapsProperty, value);
		}
	}
}