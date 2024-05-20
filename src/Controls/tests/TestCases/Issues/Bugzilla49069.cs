using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 49069, "Java.Lang.ArrayIndexOutOfBoundsException when rendering long Label on Android", PlatformAffected.Default)]
	public class Bugzilla49069 : ContentPage
	{
		public Bugzilla49069()
		{
			Label longLabelWithHorizontalTextAlignmentOfEndAndHeadTruncation = new Label
			{
				AutomationId = "lblLong",
				TextColor = Colors.Black,
				BackgroundColor = Colors.Pink,
				Text = "This is a long string that should hopefully truncate. It has HeadTruncation enabled and HorizontalTextAlignment = End",
				LineBreakMode = LineBreakMode.HeadTruncation,
				HorizontalTextAlignment = TextAlignment.End
			};

			StackLayout vslOuterPage = new StackLayout
			{
				BackgroundColor = Colors.White, // viewModel.PageBackgroundColor,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Margin = new Thickness(0, 0, 0, 0), // gets rid of the white
				Padding = new Thickness(0, 10, 0, 10),
				Spacing = 0,
				Children =
					{
						longLabelWithHorizontalTextAlignmentOfEndAndHeadTruncation,
					}
			};

			ScrollView sv = new ScrollView
			{
				Content = vslOuterPage,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Orientation = ScrollOrientation.Vertical
			};

			Content = sv;
		}
	}
}
