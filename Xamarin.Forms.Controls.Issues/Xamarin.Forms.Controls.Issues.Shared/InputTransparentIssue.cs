using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 30000, "InputTransparentIssue")]
	public class InputTransparentIssue : TestContentPage
    {
		protected override void Init ()
		{
			var abs = new AbsoluteLayout();
			var box = new BoxView { Color = Color.Red };
			var label = new Label { BackgroundColor = Color.Green , InputTransparent = true };
			abs.Children.Add(box, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
			abs.Children.Add(label, new Rectangle(0, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize), AbsoluteLayoutFlags.PositionProportional);
			label.Text = DateTime.Now.ToString();
			box.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(() =>
						{
							label.Text = DateTime.Now.ToString();
						})
				});
			Image img = new Image { Source = ImageSource.FromFile("oasis.jpg"), InputTransparent = true };
			abs.Children.Add(img, new Rectangle(.5, .5, .5, .5), AbsoluteLayoutFlags.All);
			Content = abs ;
		}
    }
}
