using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using ButtonImagePosition = Microsoft.Maui.Controls.Button.ButtonContentLayout.ImagePosition;
using iOSSpecific = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ButtonLayoutGalleryPage : ContentPage
	{
		string _buttonText = "Text";
		string _buttonImage = "bank.png";

		Thickness? _buttonPadding;

		double _buttonFontSize = -1.0;

		double _buttonImageSpacing = 10;
		double _buttonBorderWidth = -1;
		ButtonImagePosition _buttonImagePosition = ButtonImagePosition.Left;

		Button[] _buttons;

		public ButtonLayoutGalleryPage()
			: this(VisualMarker.MatchParent)
		{
		}

		public ButtonLayoutGalleryPage(IVisual visual)
		{
			InitializeComponent();
			Visual = visual;

			_buttonFontSize = Device.GetNamedSize(NamedSize.Default, typeof(Button));

			_buttons = new[]
			{
				autosizedButton,
				explicitButton,
				explicitWidthButton,
				explicitHeightButton,
				stretchedButton
			};

			// buttons are transparent on default iOS, so we have to give them something
			if (Device.RuntimePlatform == Device.iOS)
			{
				if (Visual != VisualMarker.Material)
				{
					SetBackground(Content);

					void SetBackground(View view)
					{
						if (view is Button button && !button.IsSet(Button.BackgroundColorProperty))
							view.BackgroundColor = Colors.LightGray;

						if (view is Layout layout)
						{
							foreach (var child in layout.Children)
							{
								if (child is View childView)
									SetBackground(childView);
							}
						}
					}
				}
			}

			BindingContext = this;
		}

		public string ButtonText
		{
			get => _buttonText;
			set
			{
				_buttonText = value;
				OnPropertyChanged();
			}
		}

		public string[] ButtonImages =>
			new string[] { "<none>", "bank.png", "oasissmall.jpg", "cover1.jpg" };

		public string ButtonImage
		{
			get => _buttonImage;
			set
			{
				_buttonImage = value;
				OnPropertyChanged();
			}
		}

		public Thickness? ButtonPadding
		{
			get => _buttonPadding;
			set
			{
				if (Equals(_buttonPadding, value))
					return;

				_buttonPadding = value;
				OnPropertyChanged();

				foreach (var button in _buttons)
				{
					if (_buttonPadding != null)
						button.Padding = _buttonPadding.Value;
					else
						button.ClearValue(Button.PaddingProperty);
				}
			}
		}

		public double ButtonFontSize
		{
			get => _buttonFontSize;
			set
			{
				_buttonFontSize = value;
				OnPropertyChanged();
			}
		}

		public double ButtonImageSpacing
		{
			get => _buttonImageSpacing;
			set
			{
				_buttonImageSpacing = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ButtonImageLayout));
			}
		}

		public ButtonImagePosition[] ButtonImagePositions =>
			(ButtonImagePosition[])Enum.GetValues(typeof(ButtonImagePosition));

		public ButtonImagePosition ButtonImagePosition
		{
			get => _buttonImagePosition;
			set
			{
				_buttonImagePosition = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(ButtonImageLayout));
			}
		}

		public Button.ButtonContentLayout ButtonImageLayout =>
			new Button.ButtonContentLayout(ButtonImagePosition, ButtonImageSpacing);

		public string[] ButtonFlags =>
			new[] { "<none>", "True", "False" };

		public double ButtonBorderWidth
		{
			get => _buttonBorderWidth;
			set
			{
				_buttonBorderWidth = value;
				OnPropertyChanged();

				foreach (var button in _buttons)
				{
					if (value != -1d)
					{
						button.BorderWidth = value;
						button.BorderColor = Colors.Red;
					}
					else
					{
						button.ClearValue(Button.BorderWidthProperty);
						button.ClearValue(Button.BorderColorProperty);
					}
				}
			}
		}

		void OnButtonDefaultShadowChanged(object sender, EventArgs e)
		{
			if (sender is Picker picker)
			{
				foreach (var button in _buttons)
				{
					if (picker.SelectedItem is string item && bool.TryParse(item, out var value))
					{
						button.On<Android>().SetUseDefaultShadow(value).SetUseDefaultPadding(value);
					}
					else
					{
						button.ClearValue(AndroidSpecific.Button.UseDefaultShadowProperty);
						button.ClearValue(AndroidSpecific.Button.UseDefaultPaddingProperty);
					}
				}
			}
		}
	}
}
