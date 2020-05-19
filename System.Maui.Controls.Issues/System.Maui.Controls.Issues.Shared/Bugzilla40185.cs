using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40185, "[UWP] ContentPage does not have proper right bounds in landscape", PlatformAffected.WinRT)]
	public class Bugzilla40185 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Switch Main Page",
						Command = new Command(SwitchMainPage)
					}
				}
			};
		}

		void SwitchMainPage()
		{
			Application.Current.MainPage = new ContentPage
			{
				BackgroundColor = Color.White,
				Content = new Label
				{
					Text = "This text should be in bounds in landscape mode.",
					HorizontalTextAlignment = TextAlignment.End,
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Color.Black
				}
			};
		}
	}
}
