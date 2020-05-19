using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6614, "[Android] Tabindex Calculation crashing when calculating on Layout", PlatformAffected.Android)]
	public class Issue6614 : TestContentPage
	{
		Button _button = null;

		string _instruction1 = "Turn on Screen Reader and click me.";
		protected override void Init()
		{
			_button = new Button()
			{
				Text = _instruction1,
				Command = new Command(() =>
				{							
					if(Content is ContentView currentContentView)
					{
						var currentContent = currentContentView.Content;
						currentContentView.Content = null;
						this.Content = currentContent;
						_button.Text = "Success";
					}
					else
					{
						var currentContent = this.Content;
						var contentView = new ContentView();
						this.Content = contentView;
						contentView.Content = currentContent;
						_button.Text = "Click me one more time";
					}
				}),
				TabIndex = 1
			};

			Content = new StackLayout()
			{
				Children =
				{
					_button
				}
			};
		}
	}
}
