using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1396, 
		"Label TextAlignment is not kept when resuming application", 
		PlatformAffected.Android)]
	public class Issue1396 : TestContentPage
	{
		Label _label;

		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Tap the 'Change Text' button. Tap the Overview button. Resume the application. If the label" 
						+ " text is no longer centered, the test has failed."
			};

			var button = new Button { Text = "Change Text" };
			button.Clicked += (sender, args) =>
			{
				_label.Text = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();
			};

			_label = new Label
			{ 
				HeightRequest = 400,
				BackgroundColor = Color.Gold,
				Text = "I should be centered in the gold area",
				VerticalTextAlignment = TextAlignment.Center,
				HorizontalTextAlignment = TextAlignment.Center
			};

			var layout = new StackLayout 
			{
				Children =
				{
					instructions, 
					button,
					_label
				}
			};

			var content = new ContentPage 
			{
				Content = layout 
			};

			Content = new Label { Text = "Shouldn't see this" };

			Appearing += (sender, args) => Application.Current.MainPage = content;
		}
	}
}