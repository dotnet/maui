using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40139, "Changing the Windows 10 System Theme Color causes ListView text to disappear.",
		PlatformAffected.WinRT)]
	public class Bugzilla40139 : TestContentPage
	{
		protected override void Init()
		{
			var lv = new ListView
			{
				ItemsSource = new List<Color>
				{
					Colors.Aqua,
					Colors.Black,
					Colors.Blue,
					Colors.Fuchsia,
					Colors.Gray,
					Colors.Green,
					Colors.Lime,
					Colors.Maroon,
					Colors.Navy
				},
				BackgroundColor = Colors.Gray,
				ItemTemplate = new DataTemplate(typeof(_40139ViewCell))
			};

			var layout = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text =
							"On your machine, go to Settings -> Personalization -> Colors and change the accent color for your system. If the text of the controls in the list disappears, this test has failed."
					},
					lv
				}
			};

			Content = layout;
		}

		[Preserve(AllMembers = true)]
		public class _40139ViewCell : ViewCell
		{
			public _40139ViewCell()
			{
				var label = new Label
				{
					Text = "abc",
					VerticalOptions = LayoutOptions.Center,
					TextColor = Colors.White,
					FontFamily = "Consolas",
					FontSize = 24,
					BackgroundColor = Colors.Chartreuse
				};

				var entry = new Entry
				{
					Placeholder = "Placeholder",
					TextColor = Colors.Coral
				};

				var button = new Button
				{
					Text = "Button",
					TextColor = Colors.Coral
				};

				var layout = new StackLayout();
				layout.Children.Add(label);

				layout.Children.Add(entry);
				layout.Children.Add(button);

				var image = new Image { Source = "coffee.png" };
				layout.Children.Add(image);

				View = layout;
			}
		}
	}
}