using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5252, "iOS: The Editor and Entry placeholder default color should be the same", PlatformAffected.iOS)]

	class Issue5252 : TestContentPage
	{
		protected override void Init()
		{
			var sl = new StackLayout();
			sl.Children.Add(new Label()
			{
				Text = "iOS: The Editor and Entry placeholder default color should be the same for consistency"
			});

			var entry = new Entry()
			{
				Placeholder = "Entry placeholder",
			};
			sl.Children.Add(entry);

			var editor = new Editor()
			{
				Placeholder = "Editor placeholder",
			};
			sl.Children.Add(editor);

			sl.Children.Add(new Button()
			{
				Text = "Toggle placeholder color",
				Command = new Command(() =>
				{
					entry.PlaceholderColor = entry.PlaceholderColor == null ? Colors.Red : (Color)Entry.PlaceholderColorProperty.DefaultValue;
					editor.PlaceholderColor = editor.PlaceholderColor == null ? Colors.Red : (Color)Editor.PlaceholderColorProperty.DefaultValue;
				})
			});


			Content = new ScrollView()
			{
				Content = sl
			};
		}
	}
}
