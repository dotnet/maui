namespace Maui.Controls.Sample
{
	internal class ButtonCoreGalleryPage : CoreGalleryPage<Button>
	{
		protected override bool SupportsTapGestureRecognizer
		{
			get { return false; }
		}

		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(Button element)
		{
			element.Text = "Button";
		}

		protected override void Build()
		{
			base.Build();

			IsEnabledStateViewContainer.View.Clicked += (sender, args) => IsEnabledStateViewContainer.TitleLabel.Text += " (Tapped)";

			var borderButtonContainer = new ViewContainer<Button>(Test.Button.BorderColor,
				new Button
				{
					Text = "BorderColor",
					BackgroundColor = Colors.Transparent,
					BorderColor = Colors.Red,
					BorderWidth = 1,
				}
			);

			var borderRadiusContainer = new ViewContainer<Button>(Test.Button.BorderRadius,
				new Button
				{
					Text = "BorderRadius",
					BackgroundColor = Colors.Transparent,
					BorderColor = Colors.Red,
					CornerRadius = 20,
					BorderWidth = 1,
				}
			);

			var borderWidthContainer = new ViewContainer<Button>(Test.Button.BorderWidth,
				new Button
				{
					Text = "BorderWidth",
					BackgroundColor = Colors.Transparent,
					BorderColor = Colors.Red,
					BorderWidth = 15,
				}
			);

			var clickedContainer = new EventViewContainer<Button>(Test.Button.Clicked,
				new Button
				{
					Text = "Clicked"
				}
			);
			clickedContainer.View.Clicked += (sender, args) => clickedContainer.EventFired();

			var pressedContainer = new EventViewContainer<Button>(Test.Button.Pressed,
				new Button
				{
					Text = "Pressed"
				}
			);
			pressedContainer.View.Pressed += (sender, args) => pressedContainer.EventFired();

			var commandContainer = new ViewContainer<Button>(Test.Button.Command,
				new Button
				{
					Text = "Command",
					Command = new Command(() => DisplayActionSheetAsync("Hello Command", "Cancel", "Destroy"))
				}
			);

			var imageContainer = new ViewContainer<Button>(Test.Button.Image,
				new Button
				{
					Text = "Image",
					ImageSource = new FileImageSource { File = "bank.png" }
				}
			)
			;
			var textContainer = new ViewContainer<Button>(Test.Button.Text,
				new Button
				{
					Text = "Text"
				}
			);

			var textColorContainer = new ViewContainer<Button>(Test.Button.TextColor,
				new Button
				{
					Text = "TextColor",
					TextColor = Colors.Pink
				}
			);

			var paddingContainer = new ViewContainer<Button>(Test.Button.Padding,
				new Button
				{
					Text = "Padding",
					BackgroundColor = Colors.Red,
					Padding = new Thickness(20, 30, 60, 15)
				}
			);

			Add(borderButtonContainer);
			Add(borderRadiusContainer);
			Add(borderWidthContainer);
			Add(clickedContainer);
			Add(pressedContainer);
			Add(commandContainer);
			Add(imageContainer);
			Add(textContainer);
			Add(textColorContainer);
			Add(paddingContainer);
			//stackLayout.Children.Add (textColorContainer);
		}
	}
}