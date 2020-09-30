using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7253, "[UWP] Switch custom Renderer using custom colors throws exception", PlatformAffected.UWP)]
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	public class Issue7253 : TestContentPage
	{
		public Issue7253()
		{
			Title = "Issue 7253";
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Padding = new Thickness(12)
			};

			var instructions = new Label
			{
				Text = "If the custom Switch below is rendering without problems, the test passes."
			};

			var customSwitch = new CustomSwitch
			{
				CustomColor = Color.Red
			};

			layout.Children.Add(instructions);
			layout.Children.Add(customSwitch);

			Content = layout;
		}
	}

	[Preserve(AllMembers = true)]
	public class CustomSwitch : Switch
	{
		public static readonly BindableProperty CustomColorProperty =
			BindableProperty.Create(nameof(CustomColor), typeof(Color), typeof(CustomSwitch), Color.Black);

		public Color CustomColor
		{
			get => (Color)GetValue(CustomColorProperty);
			set => SetValue(CustomColorProperty, value);
		}
	}
}