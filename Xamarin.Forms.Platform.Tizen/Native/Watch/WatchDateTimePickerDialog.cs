using System;
using ElmSharp;
using ElmSharp.Wearable;
using ELayout = ElmSharp.Layout;
using EButton = ElmSharp.Button;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.Application;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchDateTimePickerDialog : Popup, IDateTimeDialog
	{
		ELayout _surfaceLayout;
		ELayout _datetimeLayout;
		CircleSurface _surface;
		EButton _doneButton;
		Box _container;
		string _title;
		WatchDateTimePicker _picker;

		public WatchDateTimePickerDialog(EvasObject parent) : base(parent)
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

			_picker = new WatchDateTimePicker(parent, _surface);
			_picker.Show();
			_datetimeLayout.SetContent(_picker);

			_doneButton = new Button(parent)
			{
				Text = "Set",
				Style = "bottom",
			};
			_datetimeLayout.SetPartContent("elm.swallow.btn", _doneButton);
			_doneButton.Clicked += OnDoneClicked;

			ActivateRotaryInteraction();

			_datetimeLayout.Show();
			_surfaceLayout.Show();
			_container.Show();

			SetContent(_container);
			ShowAnimationFinished += OnShowAnimationFinished;
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

		public DateTimePickerMode Mode
		{
			get => _picker.Mode;
			set => _picker.Mode = value;
		}

		public DateTime MaximumDateTime
		{
			get => _picker.MaximumDateTime;
			set => _picker.MaximumDateTime = value;
		}

		public DateTime MinimumDateTime
		{
			get => _picker.MinimumDateTime;
			set => _picker.MinimumDateTime = value;
		}

		public DateTime DateTime
		{
			get => _picker.DateTime;
			set => _picker.DateTime = value;
		}

		public event EventHandler<DateChangedEventArgs> DateTimeChanged;
		public event EventHandler PickerOpened;
		public event EventHandler PickerClosed;

		protected virtual void ActivateRotaryInteraction()
		{
			if (_picker is IRotaryInteraction ri)
			{
				if (Specific.GetUseBezelInteraction(Application.Current))
				{
					ri.RotaryWidget.Activate();
				}
			}
		}

		void OnContainerLayout()
		{
			_surfaceLayout.Geometry = _container.Geometry;
			_datetimeLayout.Geometry = _container.Geometry;
		}

		void OnDoneClicked(object sender, EventArgs e)
		{
			System.Console.WriteLine("Done clicked");
			System.Console.WriteLine("Picker.DateTime - {0} ", _picker.DateTime);
			DateTimeChanged?.Invoke(this, new DateChangedEventArgs(_picker.DateTime));
			Hide();
			PickerClosed?.Invoke(this, EventArgs.Empty);
		}

		void OnBackButtonPressed(object sender, EventArgs e)
		{
			Hide();
			PickerClosed?.Invoke(this, EventArgs.Empty);
		}

		void OnShowAnimationFinished(object sender, EventArgs e)
		{
			PickerOpened?.Invoke(this, EventArgs.Empty);
		}
	}
}
