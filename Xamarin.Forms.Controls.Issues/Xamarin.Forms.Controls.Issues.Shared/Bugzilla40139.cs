using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
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
					Color.Aqua,
					Color.Black,
					Color.Blue,
					Color.Fuchsia,
					Color.Gray,
					Color.Green,
					Color.Lime,
					Color.Maroon,
					Color.Navy
				},
				BackgroundColor = Color.Gray,
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
					TextColor = Color.White,
					FontFamily = "Consolas",
					FontSize = 24,
					BackgroundColor = Color.Chartreuse
				};

				var entry = new Entry
				{
					Placeholder = "Placeholder",
					TextColor = Color.Coral
				};

				var button = new Button
				{
					Text = "Button",
					TextColor = Color.Coral
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