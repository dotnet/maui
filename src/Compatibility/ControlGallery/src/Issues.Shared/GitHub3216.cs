using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3216, "[macOS] Editor still displays white background when BackgroundColor set to Transparent", PlatformAffected.macOS)]
	public class GitHub3216 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.BurlyWood
			};

			var defaultEditor = new Editor
			{
				Text = "I'm a default Editor!"
			};

			var transparentEditor = new Editor
			{
				Text = "I'm a transparent Editor!",
				BackgroundColor = Colors.Transparent
			};
			Grid.SetRow(transparentEditor, 1);

			grid.Children.Add(defaultEditor);
			grid.Children.Add(transparentEditor);

			Content = grid;
		}
	}
}
