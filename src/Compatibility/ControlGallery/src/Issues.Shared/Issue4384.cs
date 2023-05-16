using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 4384, "Focus All The Things", PlatformAffected.Android)]
	public class Issue4384 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Tap the rotate button. The focus should cycle between the various "
				+ "controls. When each control focuses, it should display the appropriate soft keyboard. If the soft "
				+ "keyboard does not display, the text of the control clears, or the wrong keyboard displays, the test has failed."
			};

			var elements = new List<View>
			{
				instructions,
				new Entry { Keyboard = Keyboard.Email, Text = "Email" },
				new Entry { Keyboard = Keyboard.Numeric, Text = "Numeric" },
				new Editor { Keyboard = Keyboard.Email, Text = "Email" },
				new Editor { Keyboard = Keyboard.Numeric, Text = "Numeric" },
				new SearchBar { Keyboard = Keyboard.Email, Text = "Email" },
				new SearchBar { Keyboard = Keyboard.Numeric, Text = "Numeric" },
			};

			elements.ForEach(e => layout.Children.Add(e));

			int i = 1;
			layout.Children.Insert(0, new Button
			{
				Text = "rotate",
				Command = new Command(() =>
				{
					if (i >= elements.Count)
					{
						i = 1;
					}

					elements[i].Focus();
					i++;
				})
			});

			Content = layout;
		}
	}
}
