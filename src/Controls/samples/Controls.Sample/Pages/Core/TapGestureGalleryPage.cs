using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class TapGestureGalleryPage : BasePage
	{
		Command TapCommand;
		Label changeColorBoxView;
		Label windowPosition = new Label();
		Label relativeToToggleButtonPosition = new Label();
		Label relativeToContainerPosition = new Label();
		Button toggleButton;
		int clickCount = 0;

		public TapGestureGalleryPage()
		{
			TapCommand = new Command<Color>(HandleTapCommand);

			var vertical = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Spacing = 40
			};

			vertical.Add(windowPosition);
			vertical.Add(relativeToToggleButtonPosition);
			vertical.Add(relativeToContainerPosition);

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
			singleTapGesture.Tapped += OnTapped;
			singleTapLabel.GestureRecognizers.Add(singleTapGesture);
			horizontal.Add(singleTapLabel);

			var doubleTapLabel = new Label
			{
				Text = "Double Tap me!! Right Clicking Me Shouldn't Do Anything!",
				BackgroundColor = Colors.Aqua
			};
			var doubleTapGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Aqua,
				NumberOfTapsRequired = 2,
				Buttons = ButtonsMask.Secondary | ButtonsMask.Primary
			};
			doubleTapGesture.Tapped += OnTapped;
			doubleTapLabel.GestureRecognizers.Add(doubleTapGesture);
			horizontal.Add(doubleTapLabel);

			var horizontal2 = new HorizontalStackLayout
			{
				Spacing = 20,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			vertical.Add(horizontal2);


			var rightClickLabel = new Label
			{
				Text = "Right click me¡",
				BackgroundColor = Colors.Coral
			};
			var rightClickGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Coral,
				NumberOfTapsRequired = 1,
				Buttons = ButtonsMask.Secondary
			};
			rightClickGesture.Tapped += OnTapped;
			rightClickLabel.GestureRecognizers.Add(rightClickGesture);
			horizontal2.Children.Add(rightClickLabel);

			var rightorLeftClickLabel = new Label
			{
				Text = "Right or Left click me¡",
				BackgroundColor = Colors.Green
			};

			var rightOrLeftClickGesture = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Green,
				NumberOfTapsRequired = 1,
				Buttons = ButtonsMask.Secondary | ButtonsMask.Primary
			};

			rightOrLeftClickGesture.Tapped += OnTapped;
			rightorLeftClickLabel.GestureRecognizers.Add(rightOrLeftClickGesture);
			horizontal2.Children.Add(rightorLeftClickLabel);

			var rightOrLeftClickLabel2Taps = new Label
			{
				Text = "Double Click Me! ",
				BackgroundColor = Colors.Green,
			};

			var rightOrLeftClickGesture2Taps = new TapGestureRecognizer
			{
				Command = TapCommand,
				CommandParameter = Colors.Green,
				NumberOfTapsRequired = 2,
				Buttons = ButtonsMask.Secondary | ButtonsMask.Primary
			};

			rightOrLeftClickGesture2Taps.Tapped += OnTapped;
			rightOrLeftClickLabel2Taps.GestureRecognizers.Add(rightOrLeftClickGesture2Taps);
			horizontal2.Children.Add(rightOrLeftClickLabel2Taps);

			changeColorBoxView = new Label
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				WidthRequest = 200,
				HeightRequest = 50,
				Text = "Tap Gesture Gallery"
			};
			vertical.Add(changeColorBoxView);

			toggleButton = new Button()
			{
				Text = "Toggle Single Tap Gesture",
				Command = new Command(() =>
				{
					if (singleTapLabel.GestureRecognizers.Count > 0)
						singleTapLabel.GestureRecognizers.RemoveAt(0);
					else
						singleTapLabel.GestureRecognizers.Add(singleTapGesture);
				})
			};

			vertical.Add(toggleButton);

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
			tripleClickGesture.Tapped += OnTapped;
			tripleClicklabel.GestureRecognizers.Add(tripleClickGesture);
			horizontal.Children.Add(tripleClicklabel);

			Content = vertical;
		}

		void OnTapped(object? sender, System.EventArgs e)
		{
			var args = (TappedEventArgs)e;
			var view = (View)sender!;

			windowPosition.Text = $"Position inside window: {args.GetPosition(null)}";
			relativeToToggleButtonPosition.Text = $"Position relative to toggle button: {args.GetPosition(toggleButton)}";
			relativeToContainerPosition.Text = $"Position inside my view: {args.GetPosition(view)}";
		}

		void HandleTapCommand(Color backgroundColor)
		{
			changeColorBoxView.BackgroundColor = backgroundColor;
			changeColorBoxView.Text = $"Tap Gesture Gallery: {clickCount}";
			clickCount++;
		}
	}
}
