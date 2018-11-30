using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ButtonGallery : ContentPage
	{
		public ButtonGallery ()
		{
			//ShellAppearance.SetNavBarVisible(this, false);
			Shell.SetSearchHandler(this, new SearchHandler() { SearchBoxVisibility = SearchBoxVisiblity.Collapsable });
			BackgroundColor = new Color (0.9);

			var normal = new Button { Text = "Normal Button" };
			normal.Effects.Add (Effect.Resolve ($"{Issues.Effects.ResolutionGroupName}.BorderEffect"));

			var disabled = new Button { Text = "Disabled Button"};
			var disabledswitch = new Switch ();
			disabledswitch.SetBinding (Switch.IsToggledProperty, "IsEnabled");
			disabledswitch.BindingContext = disabled;

			var canTapLabel = new Label {
				Text = "Cannot Tap"
			};

			disabled.Clicked += (sender, e) => {
				canTapLabel.Text = "TAPPED!";
			};

			var click = new Button { Text = "Click Button" };
			var rotate = new Button { Text = "Rotate Button" };
			var transparent = new Button { Text = "Transparent Button" };
			string fontName;
			switch (Device.RuntimePlatform) {
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

			var font = Font.OfSize (fontName, NamedSize.Medium);

			var themedButton = new Button {
				Text = "Accent Button",
				BackgroundColor = Color.Accent,
				TextColor = Color.White,
				ClassId = "AccentButton",
				Font = font
			};
			var borderButton = new Button {
				Text = "Border Button",
				BorderColor = Color.Black,
				BackgroundColor = Color.Purple,
				BorderWidth = 5,
#pragma warning disable 0618
				BorderRadius = 5
#pragma warning restore
			};
			var timer = new Button { Text = "Timer" };
			var busy = new Button { Text = "Toggle Busy" };
			var alert = new Button { Text = "Alert" };
			var alertSingle = new Button {Text = "Alert Single"};
			var image = new Button { Text = "Image Button", Image = new FileImageSource {File = "bank.png"}, BackgroundColor = Color.Blue.WithLuminosity (.8) };

			themedButton.Clicked += (sender, args) => themedButton.Font = Font.Default;

			alertSingle.Clicked += (sender, args) => DisplayAlert ("Foo", "Bar", "Cancel");

			disabled.IsEnabled = false;
			int i = 1;
			click.Clicked += (sender, e) => { click.Text = "Clicked " + i++; };
			rotate.Clicked += (sender, e) => rotate.RelRotateTo (180);
			transparent.Opacity = .5;

			int j = 1;
			timer.Clicked += (sender, args) => Device.StartTimer (TimeSpan.FromSeconds (1), () => {
				timer.Text = "Timer Elapsed " + j++;
				return j < 4;
			});

			bool isBusy = false;
			busy.Clicked += (sender, args) => IsBusy = isBusy = !isBusy;

			alert.Clicked += async (sender, args) => {
				var result = await DisplayAlert ("User Alert", "This is a user alert. This is only a user alert.", "Accept", "Cancel");
				alert.Text = result ? "Accepted" : "Cancelled";
			};

			borderButton.Clicked += (sender, args) => borderButton.BackgroundColor = Color.Default;

			Content = new ScrollView {
				BackgroundColor = Color.Red,
				Content = new StackLayout {
					Padding = new Size (20, 20),
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
						new Button {Text = "Thin Border", BorderWidth = 1, BackgroundColor=Color.White, BorderColor = Color.Black, TextColor = Color.Black},
						new Button {Text = "Thinner Border", BorderWidth = .5, BackgroundColor=Color.White, BorderColor = Color.Black, TextColor = Color.Black},
						new Button {Text = "BorderWidth == 0", BorderWidth = 0, BackgroundColor=Color.White, BorderColor = Color.Black, TextColor = Color.Black},
						timer,
						busy,
						alert,
						alertSingle,
						image,
					}
				}
			};
				
		}
	}
}
