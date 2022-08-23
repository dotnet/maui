using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class TapGestureGalleryPage : BasePage
	{
		Command TapCommand;
		Label changeColorBoxView;

		public TapGestureGalleryPage()
		{
			TapCommand = new Command<Color>(HandleTapCommand);

			var vertical = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Spacing = 40
			};

			var horizontal = new HorizontalStackLayout
			{
				Spacing = 20,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			vertical.Add(horizontal);

			var singleTapLabel = new Label
			{
				Text = "Tap me!",
				BackgroundColor = Colors.PaleGreen
			};
			var singleTapGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.PaleGreen,
				NumberOfTapsRequired = 1,
			};
			singleTapLabel.GestureRecognizers.Add(singleTapGesture);
			horizontal.Add(singleTapLabel);

			var doubleTapLabel = new Label
			{
				Text = "Double Tap me!!",
				BackgroundColor = Colors.Aqua
			};
			var doubleTapGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Aqua,
				NumberOfTapsRequired = 2,
			};
			doubleTapLabel.GestureRecognizers.Add(doubleTapGesture);
			horizontal.Add(doubleTapLabel);

			changeColorBoxView = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 200,
				HeightRequest = 50,
				Text = "Tap Gesture Gallery"
			};
			vertical.Add(changeColorBoxView);


			vertical.Add(new Button()
			{
				Text = "Toggle Single Tap Gesture",
				Command = new Command(() =>
				{
					if (singleTapLabel.GestureRecognizers.Count > 0)
						singleTapLabel.GestureRecognizers.RemoveAt(0);
					else
						singleTapLabel.GestureRecognizers.Add(singleTapGesture);
				})
			});

			var tripleClicklabel = new Label
			{
				Text = "Triple click me!!!",
				BackgroundColor = Colors.Olive
			};
			var tripleClickGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Olive,
				NumberOfTapsRequired = 3,
				Buttons = ButtonsMask.Primary
			};
			tripleClicklabel.GestureRecognizers.Add(tripleClickGesture);
			horizontal.Children.Add(tripleClicklabel);

			var rightClickLabel = new Label
			{
				Text = "Right click me¡",
				BackgroundColor = Colors.Coral
			};
			var rigthClickGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Coral,
				NumberOfTapsRequired = 1,
				Buttons = ButtonsMask.Secondary
			};
			rightClickLabel.GestureRecognizers.Add(rigthClickGesture);
			horizontal.Children.Add(rightClickLabel);


			var rightorLeftClickLabel = new Label
			{
				Text = "Right or Left click me¡",
				BackgroundColor = Colors.Green
			};
			var rigthOrLeftClickGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Green,
				NumberOfTapsRequired = 1,
				Buttons = ButtonsMask.Secondary | ButtonsMask.Primary
			};
			rightorLeftClickLabel.GestureRecognizers.Add(rigthOrLeftClickGesture);
			horizontal.Children.Add(rightorLeftClickLabel);


			Content = vertical;
		}

		void HandleTapCommand(Color backgroundColor)
		{
			changeColorBoxView.BackgroundColor = backgroundColor;
		}
	}
}
