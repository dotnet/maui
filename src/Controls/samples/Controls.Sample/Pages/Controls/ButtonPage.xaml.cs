using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ButtonPage
	{
		int _count;
		bool _useGradientBrush;

		public ButtonPage()
		{
			InitializeComponent();
			UpdateButtonBrush();

			BindingContext = new ButtonPageViewModel();
		}

		void OnButtonClicked(object? sender, System.EventArgs e)
		{
			Debug.WriteLine("Clicked");
		}

		void Button_Clicked(System.Object? sender, System.EventArgs e)
		{
			if (ImageSourceButton.ImageSource is null)
			{
				ImageSourceButton.ImageSource = "settings.png";
			}
			else
			{
				ImageSourceButton.ImageSource = null;
			}
		}

		void OnPositionChange(object? sender, System.EventArgs e)
		{
			var newPosition = ((int)positionChange.ContentLayout.Position) + 1;

			if (newPosition >= 4)
				newPosition = 0;

			positionChange.ContentLayout =
				new Button.ButtonContentLayout((Button.ButtonContentLayout.ImagePosition)newPosition,
					positionChange.ContentLayout.Spacing);
		}

		void OnDecreaseSpacing(object? sender, System.EventArgs e)
		{
			positionChange.ContentLayout =
				new Button.ButtonContentLayout(positionChange.ContentLayout.Position,
					positionChange.ContentLayout.Spacing - 1);
		}

		void OnIncreasingSpacing(object? sender, System.EventArgs e)
		{
			positionChange.ContentLayout =
				new Button.ButtonContentLayout(positionChange.ContentLayout.Position,
					positionChange.ContentLayout.Spacing + 1);
		}

		void OnLineBreakModeButtonClicked(object? sender, System.EventArgs e)
		{
			LineBreakModeButton.LineBreakMode = ImageLineBreakModeButton.LineBreakMode = SelectLineBreakMode();
		}

		LineBreakMode SelectLineBreakMode()
		{
			_count++;
			switch (_count)
			{
				case 1:
					return LineBreakMode.CharacterWrap;
				case 2:
					return LineBreakMode.HeadTruncation;
				case 3:
					return LineBreakMode.MiddleTruncation;
				case 4:
					return LineBreakMode.NoWrap;
				case 5:
					return LineBreakMode.TailTruncation;
				default:
					_count = 0;
					return LineBreakMode.WordWrap;
			}
		}

		int _backgroundCount;

		void OnBackgroundButtonClicked(object? sender, System.EventArgs e)
		{
			BackgroundButton.Text = $"Background tapped {_backgroundCount} times";
			_backgroundCount++;
		}

		void OnChangeBrushButtonClicked(System.Object? sender, System.EventArgs e)
		{
			UpdateButtonBrush();
		}

		void UpdateButtonBrush()
		{
			var solidColorBrush = new SolidColorBrush(Colors.Red);

			var linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Orange, Offset = 0.2f },
					new GradientStop { Color = Colors.OrangeRed, Offset = 0.8f }
				}
			};

			if (_useGradientBrush)
			{
				ChangeBrushButton.Background = linearGradientBrush;
				_useGradientBrush = false;
			}
			else
			{
				ChangeBrushButton.Background = solidColorBrush;
				_useGradientBrush = true;
			}
		}
	}

	public class ButtonPageViewModel : BindableObject
	{
		bool _changeButtonColor;

		public string ButtonBackground => "#fc87ad";

		public bool ChangeButtonColor
		{
			get => _changeButtonColor;
			set
			{
				if (_changeButtonColor != value)
				{
					_changeButtonColor = value;
					OnPropertyChanged();
				}
			}
		}

		public ICommand ButtonCommand => new Command(OnExecuteImageButtonCommand);

		void OnExecuteImageButtonCommand()
		{
			Debug.WriteLine("Command");
		}
	}
}