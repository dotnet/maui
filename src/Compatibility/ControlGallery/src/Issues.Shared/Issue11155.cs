using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.Frame)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11155,
		"Picking option in picker crashes application - iOS ONLY",
		PlatformAffected.iOS)]
	public class Issue11155 : TestContentPage
	{
		public Issue11155()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11155";

			var layout = new StackLayout
			{
				Padding = 12
			};

			var instructions = new Label
			{
				Text = "Select any Picker item and tap the Done Button."
			};

			var picker = new Picker
			{
				ItemsSource = new List<string>
				{
					"Item 1",
					"Item 2",
					"Item 3"
				}
			};

			layout.Children.Add(instructions);
			layout.Children.Add(picker);

			Content = layout;

			picker.SelectedIndexChanged += (sender, args) =>
			{
				layout.Children.Clear();

				var newInstructions = new Label
				{
					Padding = 12,
					BackgroundColor = Colors.Black,
					TextColor = Colors.White,
					Text = "If you can read this text, the test has passed."
				};

				layout.Children.Add(newInstructions);

			};
		}
	}
}