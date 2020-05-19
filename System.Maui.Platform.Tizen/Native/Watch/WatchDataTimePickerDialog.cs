using System;
using ElmSharp;
using ElmSharp.Wearable;
using ELayout = ElmSharp.Layout;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchDataTimePickerDialog : Popup, IDateTimeDialog
	{
		ELayout _surfaceLayout;
		ELayout _datetimeLayout;
		CircleSurface _surface;
		EButton _doneButton;
		Box _container;
		string _title;

		public WatchDataTimePickerDialog(EvasObject parent) : base(parent)
		{
			AlignmentX = -1;
			AlignmentY = -1;
			WeightX = 1.0;
			WeightY = 1.0;
			Style = "circle";

			_container = new Box(parent) { AlignmentX = -1, AlignmentY = -1, WeightX = 1, WeightY = 1 };
			_container.BackgroundColor = ElmSharp.Color.Blue;
			_container.SetLayoutCallback(OnContainerLayout);

			_datetimeLayout = new ELayout(parent);
			_surfaceLayout = new ELayout(parent);

			_container.PackEnd(_datetimeLayout);
			_container.PackEnd(_surfaceLayout);

			_datetimeLayout.SetTheme("layout", "circle", "datetime");
			_surface = new CircleSurface(_surfaceLayout);

			WatchPicker = new WatchDateTimePicker(parent, _surface);
			WatchPicker.Show();
			_datetimeLayout.SetContent(WatchPicker);

			_doneButton = new Button(parent)
			{
				Text = "Set",
				Style = "bottom",
			};
			_datetimeLayout.SetPartContent("elm.swallow.btn", _doneButton);
			_doneButton.Clicked += OnDoneClicked;

			((IRotaryActionWidget)WatchPicker.CircleSelector).Activate();

			_datetimeLayout.Show();
			_surfaceLayout.Show();
			_container.Show();

			SetContent(_container);
			BackButtonPressed += OnBackButtonPressed;
		}

		public string Title
		{
			get => _title;
			set
			{
				_title = value;
				_datetimeLayout.SetPartText("elm.text", _title);
			}
		}

		public DateTimePicker Picker => WatchPicker;
		protected WatchDateTimePicker WatchPicker { get; }

		public event EventHandler<DateChangedEventArgs> DateTimeChanged;

		void OnContainerLayout()
		{
			_surfaceLayout.Geometry = _container.Geometry;
			_datetimeLayout.Geometry = _container.Geometry;
		}

		void OnDoneClicked(object sender, EventArgs e)
		{
			System.Console.WriteLine("Done clicked");
			System.Console.WriteLine("Picker.DateTime - {0} ", Picker.DateTime);
			DateTimeChanged?.Invoke(this, new DateChangedEventArgs(Picker.DateTime));
			Hide();
		}

		void OnBackButtonPressed(object sender, EventArgs e)
		{
			Hide();
		}
	}
}
