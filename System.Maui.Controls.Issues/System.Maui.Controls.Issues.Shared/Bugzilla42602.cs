using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.BoxView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 42602, "[Win] Custom BoxView Renderer Does Not Render All Its Children Elements", PlatformAffected.WinRT)]
	public class Bugzilla42602 : TestContentPage
	{
		AbsoluteLayout content;

		protected override void Init()
		{
			//background white 800 x 600 square
			content = new AbsoluteLayout()
			{
				BackgroundColor = Color.White,
				WidthRequest = 800,
				HeightRequest = 800,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			//test TextBoxView 400 x 400, color gray, should have a red ellipse and a red "hello world"

			var test = new TextBoxView() { WidthRequest = 300, HeightRequest = 300, BackgroundColor = Color.Blue };
			content.Children.Add(test, new Point((content.WidthRequest - test.WidthRequest) / 2f, (content.HeightRequest - test.HeightRequest) / 2f));

			Content = content;
		}

		public class TextBoxView : BoxView
		{

		}
	}
}
