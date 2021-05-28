using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class ButtonGallery : ContentPage
	{
		public ButtonGallery()
		{
			//ShellAppearance.SetNavBarVisible(this, false);
			Shell.SetSearchHandler(this, new SearchHandler() { SearchBoxVisibility = SearchBoxVisibility.Collapsible });
			BackgroundColor = new Color(0.9f);

			var normal = new Button { Text = "Normal Button" };
			normal.Effects.Add(Effect.Resolve($"{Issues.Effects.ResolutionGroupName}.BorderEffect"));

			var disabled = new Button { Text = "Disabled Button" };
			var disabledswitch = new Switch();
			disabledswitch.SetBinding(Switch.IsToggledProperty, "IsEnabled");
			disabledswitch.BindingContext = disabled;

			var canTapLabel = new Label
			{
				Text = "Cannot Tap"
			};

			disabled.Clicked += (sender, e) =>
			{
				canTapLabel.Text = "TAPPED!";
			};

			var click = new Button { Text = "Click Button" };
			var rotate = new Button { Text = "Rotate Button" };
			var transparent = new Button { Text = "Transparent Button" };
			string fontName;
			switch (Device.RuntimePlatform)
			{
				default:
				case Device.iOS:
					fontName = "Georgia";
					break;
				case Device.Android:
					fontName = "sans-serif-light";
					break;
				case Device.UWP:
					fontName = "Comic Sans MS";
					break;
			}

			var font = Font.OfSize(fontName, Device.GetNamedSize(NamedSize.Medium, typeof(Button), false));

			var themedButton = new Button
			{
				Text = "Accent Button",
				//BackgroundColor = Colors.Accent,
				TextColor = Colors.White,
				ClassId = "AccentButton",
				Font = font
			};
			var borderButton = new Button
			{
				Text = "Border Button",
				BorderColor = Colors.Black,
				BackgroundColor = Colors.Purple,
				BorderWidth = 5,
				CornerRadius = 5
			};
			var timer = new Button { Text = "Timer" };
			var busy = new Button { Text = "Toggle Busy" };
			var alert = new Button { Text = "Alert" };
			var alertSingle = new Button { Text = "Alert Single" };
			var image = new Button { Text = "Image Button", ImageSource = new FileImageSource { File = "bank.png" }, BackgroundColor = Colors.Blue.WithLuminosity(.8f) };
			AutomationProperties.SetName(image, "Image Automation Name");
			var gif = new Button { ImageSource = "GifOne.gif" };
			var automation = new Button { Text = "Display Name" };
			AutomationProperties.SetName(automation, "Automation Name");
			var labeledBy = new Button { Text = "Labeled By" };
			var autoLabel = new Label { Text = "Label Text" };
			AutomationProperties.SetLabeledBy(labeledBy, autoLabel);

			themedButton.Clicked += (sender, args) => themedButton.Font = Font.Default;

			alertSingle.Clicked += (sender, args) => DisplayAlert("Foo", "Bar", "Cancel");

			disabled.IsEnabled = false;
			int i = 1;
			click.Clicked += (sender, e) => { click.Text = "Clicked " + i++; };
			rotate.Clicked += (sender, e) => rotate.RelRotateTo(180);
			transparent.Opacity = .5;

			int j = 1;
			timer.Clicked += (sender, args) => Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				timer.Text = "Timer Elapsed " + j++;
				return j < 4;
			});

			bool isBusy = false;
			busy.Clicked += (sender, args) => IsBusy = isBusy = !isBusy;

			alert.Clicked += async (sender, args) =>
			{
				var result = await DisplayAlert("User Alert", "This is a user alert. This is only a user alert.", "Accept", "Cancel");
				alert.Text = result ? "Accepted" : "Cancelled";
			};

			borderButton.Clicked += (sender, args) => borderButton.BackgroundColor = null;

			Content = new ScrollView
			{
				BackgroundColor = Colors.Red,
				Content = new StackLayout
				{
					Padding = new Size(20, 20),
					Children = {
						normal,
						new StackLayout {
							Orientation = StackOrientation.Horizontal,
							Children={
								disabled,
								disabledswitch,
							},
						},
						canTapLabel,
						click,
						rotate,
						transparent,
						themedButton,
						borderButton,
						new Button {Text = "Thin Border", BorderWidth = 1, BackgroundColor=Colors.White, BorderColor = Colors.Black, TextColor = Colors.Black},
						new Button {Text = "Thinner Border", BorderWidth = .5, BackgroundColor=Colors.White, BorderColor = Colors.Black, TextColor = Colors.Black},
						new Button {Text = "BorderWidth == 0", BorderWidth = 0, BackgroundColor=Colors.White, BorderColor = Colors.Black, TextColor = Colors.Black},
						timer,
						busy,
						alert,
						alertSingle,
						image,
						gif,
						automation,
						autoLabel,
						labeledBy
					}
				}
			};

		}
	}
}
