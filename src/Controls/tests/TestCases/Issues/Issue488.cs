using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 488, "Resizing the Label results in wrapped text being cropped on iOS", PlatformAffected.iOS)]
	public class Issue488 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new Microsoft.Maui.Controls.Compatibility.RelativeLayout
			{
				BackgroundColor = Colors.Gray
			};
			var label = new Label
			{
				Text = "I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text. I am a long bit of text."
			};
			layout.Children.Add(label, () => new Rect(0, 0, 250, 400));
			Content = layout;
		}
	}
}
