using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.BoxView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
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
				BackgroundColor = Colors.White,
				WidthRequest = 800,
				HeightRequest = 800,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand
			};

			//test TextBoxView 400 x 400, color gray, should have a red ellipse and a red "hello world"

			var test = new TextBoxView() { WidthRequest = 300, HeightRequest = 300, BackgroundColor = Colors.Blue };
			content.Children.Add(test, new Point((content.WidthRequest - test.WidthRequest) / 2f, (content.HeightRequest - test.HeightRequest) / 2f));

			Content = content;
		}

		public class TextBoxView : BoxView
		{

		}
	}
}
