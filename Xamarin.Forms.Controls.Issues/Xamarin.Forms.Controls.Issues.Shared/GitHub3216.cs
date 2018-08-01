using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3216, "[macOS] Editor still displays white background when BackgroundColor set to Transparent", PlatformAffected.macOS)]
	public class GitHub3216 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid
			{
				BackgroundColor = Color.BurlyWood
			};

			var defaultEditor = new Editor
			{
				Text = "I'm a default Editor!"
			};

			var transparentEditor = new Editor
			{
				Text = "I'm a transparent Editor!",
				BackgroundColor = Color.Transparent
			};
			Grid.SetRow(transparentEditor, 1);

			grid.Children.Add(defaultEditor);
			grid.Children.Add(transparentEditor);

			Content = grid;
		}
	}
}
