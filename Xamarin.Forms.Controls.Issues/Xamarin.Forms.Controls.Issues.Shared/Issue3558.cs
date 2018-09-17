using System.Diagnostics;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3558, "[WPF] Label displays only one line", PlatformAffected.WPF)]
	public class Issue3558 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();
			Label label = new Label { Text = "Lotsa text here to make this a really really really really long line that should be wrapped",
				LineBreakMode = LineBreakMode.WordWrap };

			stack.Children.Add(label);
			Content = stack;
		}
	}
}
