using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8272, "[Bug] TextDecorations don't work", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Label)]
#endif
	public class Issue8272 : TestContentPage
	{
		public Issue8272()
		{
#if APP
			Title = "Issue 8272";
#endif
		}

		protected override void Init()
		{
			var layout = new StackLayout
			{
				Padding = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Toggle the Switch. If the text below is underline, the test has passed."
			};

			Grid grid = new Grid { Margin = new Thickness(20) };
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

			ScrollView scrollView = new ScrollView();
			Label label = new Label
			{
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				CharacterSpacing = 2,
				Text = "The Switch should toggle underline on this Label."
			};
			scrollView.Content = label;

			Label underlineLabel = new Label { Text = "Underline:", VerticalOptions = LayoutOptions.Center };
			Switch underlineSwitch = new Switch { VerticalOptions = LayoutOptions.Center };

			Label characterSpacingLabel = new Label { Text = "CharacterSpacing:", VerticalOptions = LayoutOptions.Center };
			Slider characterSpacingSlider = new Slider
			{
				Maximum = 24,
				Minimum = 0,
				Value = 2,
				VerticalOptions = LayoutOptions.Center
			};

			grid.Children.Add(underlineLabel, 0, 0);
			grid.Children.Add(underlineSwitch, 1, 0);
			grid.Children.Add(characterSpacingLabel, 0, 1);
			grid.Children.Add(characterSpacingSlider, 1, 1);
			grid.Children.Add(scrollView, 0, 2);
			Grid.SetColumnSpan(scrollView, 4);

			layout.Children.Add(instructions);
			layout.Children.Add(grid);

			underlineSwitch.Toggled += (sender, args) =>
			{
				if (underlineSwitch.IsToggled)
					label.TextDecorations = TextDecorations.Underline;
				else
					label.TextDecorations = TextDecorations.None;
			};

			characterSpacingSlider.ValueChanged += (sender, args) =>
			{
				label.CharacterSpacing = characterSpacingSlider.Value;
			};

			Content = layout;
		}
	}
}