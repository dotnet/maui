﻿using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1909, "Xamarin.forms 2.5.0.280555 and android circle button issue", PlatformAffected.Android)]
	public class Issue1909 : TestContentPage
	{
		public class FlatButton : Button { }
		protected override void Init()
		{
			Button button = new Button
			{
				BackgroundColor = Colors.Red,
				CornerRadius = 32,
				BorderWidth = 0,
				FontSize = 36,
				HeightRequest = 64,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 64,
				AutomationId = "TestReady"
			};

			button.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetUseDefaultPadding(true).SetUseDefaultShadow(true);

			FlatButton flatButton = new FlatButton
			{
				BackgroundColor = Colors.Red,
				CornerRadius = 32,
				BorderWidth = 0,
				FontSize = 36,
				HeightRequest = 64,
				HorizontalOptions = LayoutOptions.Center,
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 64
			};

			Content = new StackLayout
			{
				Children = {
					new Label{ Text = "The following buttons should be perfectly round. The bottom button should be larger and should not have a shadow." },
					button,
					flatButton
				}
			};
		}
	}
}