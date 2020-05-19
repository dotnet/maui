using System;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7825, "WPF Frame cornerRadius doesn't clip content", PlatformAffected.WPF)]
	public class Issue7825 : ContentPage
	{
		public Issue7825()
		{
			var sl = new StackLayout { Padding = new Size(20, 20) };

			var frame = new Frame {
				Content = new BoxView() {
					BackgroundColor = Color.Red
				},
				VerticalOptions = LayoutOptions.FillAndExpand
			};
			frame.Padding = 0;
			frame.CornerRadius = 50;

			sl.Children.Add(frame);
			Content = sl;
		}
	}
}

