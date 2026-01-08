using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class LongPressGestureGalleryPage : BasePage
	{
		Command<string> LongPressCommand;
		Label statusLabel;
		Label stateLabel;
		Label positionLabel;
		int longPressCount = 0;

		public LongPressGestureGalleryPage()
		{
			LongPressCommand = new Command<string>(HandleLongPressCommand);

			var vertical = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 20
			};

			// Status labels
			statusLabel = new Label
			{
				Text = "Long press any box below...",
				FontSize = 18,
				FontAttributes = FontAttributes.Bold,
				HorizontalTextAlignment = TextAlignment.Center
			};
			vertical.Add(statusLabel);

			stateLabel = new Label
			{
				Text = "State: None",
				FontSize = 14,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Colors.Gray
			};
			vertical.Add(stateLabel);

			positionLabel = new Label
			{
				Text = "Position: -",
				FontSize = 14,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Colors.Gray
			};
			vertical.Add(positionLabel);

			// Basic long press
			var basicBox = new Frame
			{
				BackgroundColor = Colors.LightBlue,
				Padding = 20,
				CornerRadius = 10,
				HeightRequest = 100
			};
			var basicLabel = new Label
			{
				Text = "Basic Long Press\n(500ms default)",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 16
			};
			basicBox.Content = basicLabel;

			var basicGesture = new LongPressGestureRecognizer
			{
				Command = LongPressCommand,
				CommandParameter = "Basic"
			};
			basicGesture.LongPressed += OnLongPressed;
			basicGesture.LongPressing += OnLongPressing;
			basicBox.GestureRecognizers.Add(basicGesture);
			vertical.Add(basicBox);

			// Custom duration
			var customBox = new Frame
			{
				BackgroundColor = Colors.LightGreen,
				Padding = 20,
				CornerRadius = 10,
				HeightRequest = 100
			};
			var customLabel = new Label
			{
				Text = "Custom Duration\n(1000ms)",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 16
			};
			customBox.Content = customLabel;

			var customGesture = new LongPressGestureRecognizer
			{
				Command = LongPressCommand,
				CommandParameter = "Custom (1000ms)",
				MinimumPressDuration = 1000
			};
			customGesture.LongPressed += OnLongPressed;
			customGesture.LongPressing += OnLongPressing;
			customBox.GestureRecognizers.Add(customGesture);
			vertical.Add(customBox);

			// Sensitive to movement
			var sensitiveBox = new Frame
			{
				BackgroundColor = Colors.LightCoral,
				Padding = 20,
				CornerRadius = 10,
				HeightRequest = 100
			};
			var sensitiveLabel = new Label
			{
				Text = "Movement Sensitive\n(5px threshold - try dragging)",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 16
			};
			sensitiveBox.Content = sensitiveLabel;

			var sensitiveGesture = new LongPressGestureRecognizer
			{
				Command = LongPressCommand,
				CommandParameter = "Sensitive",
				AllowableMovement = 5
			};
			sensitiveGesture.LongPressed += OnLongPressed;
			sensitiveGesture.LongPressing += OnLongPressing;
			sensitiveBox.GestureRecognizers.Add(sensitiveGesture);
			vertical.Add(sensitiveBox);

			// Combined with tap
			var combinedBox = new Frame
			{
				BackgroundColor = Colors.LightYellow,
				Padding = 20,
				CornerRadius = 10,
				HeightRequest = 100
			};
			var combinedLabel = new Label
			{
				Text = "Long Press + Tap\n(Try both quick tap and long press)",
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				FontSize = 16
			};
			combinedBox.Content = combinedLabel;

			var combinedLongPress = new LongPressGestureRecognizer
			{
				Command = LongPressCommand,
				CommandParameter = "Combined (LongPress)"
			};
			combinedLongPress.LongPressed += OnLongPressed;
			combinedLongPress.LongPressing += OnLongPressing;
			combinedBox.GestureRecognizers.Add(combinedLongPress);

			var combinedTap = new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					statusLabel.Text = "Tapped! (Quick tap)";
					statusLabel.TextColor = Colors.Orange;
				})
			};
			combinedBox.GestureRecognizers.Add(combinedTap);
			vertical.Add(combinedBox);

			// Info label
			var infoLabel = new Label
			{
				Text = "💡 Tip: On Android, the system default duration (~400ms) is used instead of MinimumPressDuration.",
				FontSize = 12,
				TextColor = Colors.Gray,
				HorizontalTextAlignment = TextAlignment.Center,
				Margin = new Thickness(0, 20, 0, 0)
			};
			vertical.Add(infoLabel);

			Content = new ScrollView { Content = vertical };
		}

		void OnLongPressed(object? sender, LongPressedEventArgs e)
		{
			stateLabel.Text = $"State: {e.State}";
			stateLabel.TextColor = e.State == GestureStatus.Completed ? Colors.Green : Colors.Red;

			if (e.Parameter is View view)
			{
				positionLabel.Text = $"Position: {e.GetPosition(view)?.X:F0}, {e.GetPosition(view)?.Y:F0}";
			}
		}

		void OnLongPressing(object? sender, LongPressingEventArgs e)
		{
			stateLabel.Text = $"State: {e.State}";
			stateLabel.TextColor = e.State switch
			{
				GestureStatus.Started => Colors.Blue,
				GestureStatus.Running => Colors.Orange,
				GestureStatus.Completed => Colors.Green,
				GestureStatus.Canceled => Colors.Red,
				_ => Colors.Gray
			};
		}

		void HandleLongPressCommand(string source)
		{
			longPressCount++;
			statusLabel.Text = $"Long Pressed: {source} (Count: {longPressCount})";
			statusLabel.TextColor = Colors.Green;
		}
	}
}
