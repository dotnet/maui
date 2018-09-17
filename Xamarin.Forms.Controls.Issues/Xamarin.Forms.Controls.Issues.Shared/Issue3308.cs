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
	[Issue(IssueTracker.Github, 3308, "[WPF] Button does not recalculate width on WPF", PlatformAffected.WPF)]
	public class Issue3308 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();
			Button b1 = new Button { Text = "Button 1", HeightRequest = 100 };
			Button b2 = new Button { Text = "Button 2", HorizontalOptions = LayoutOptions.Center };
			Button b3 = new Button { Text = "Button 3", VerticalOptions = LayoutOptions.EndAndExpand };
			Button b4 = new Button { Text = "Button 4", HeightRequest = 100, VerticalOptions = LayoutOptions.End};
			b1.Clicked += (sender, e) => {
				b1.HeightRequest = 300;
				b1.Text = "Hello, I'm Button 1";
			};
			b2.Clicked += (sender, e) => {
				b2.Text = "Hello, I'm Button 2";
			};
			b3.Clicked += (sender, e) => {
				b3.Text = "Hello, I'm Button 3";
			};
			b4.Clicked += (sender, e) => {
				b4.Text = "Hello, I'm Button 4";
			};
			stack.Children.Add(b1);
			stack.Children.Add(b2);
			stack.Children.Add(b3);
			stack.Children.Add(b4);
			Content = stack;
		}
	}
}
