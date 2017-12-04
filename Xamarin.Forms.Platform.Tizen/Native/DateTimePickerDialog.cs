using System;
using ElmSharp;
using EButton = ElmSharp.Button;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public class DateTimePickerDialog : Dialog
	{
		DateTimeSelector _dateTimePicker;
		EvasObject _parent;

		/// <summary>
		/// Creates a dialog window.
		/// </summary>
		public DateTimePickerDialog(EvasObject parent) : base(parent)
		{
			_parent = parent;
			Initialize();
		}

		/// <summary>
		/// Occurs when the date of this dialog has changed.
		/// </summary>
		public event EventHandler<DateChangedEventArgs> DateTimeChanged;

		/// <summary>
		/// Gets the <see cref="DateTimePicker"/> contained in this dialog.
		/// </summary>
		public DateTimeSelector DateTimePicker
		{
			get
			{
				return _dateTimePicker;
			}
			private set
			{
				if (_dateTimePicker != value)
				{
					ApplyDateTimePicker(value);
				}
			}
		}

		/// <summary>
		/// Creates date picker in dialog window.
		/// </summary>
		public void InitializeDatePicker(DateTime date, DateTime minimumDate, DateTime maximumDate)
		{
			var datePicker = new DatePicker(this)
			{
				Date = date,
				MinimumDate = minimumDate,
				MaximumDate = maximumDate
			};
			Content = DateTimePicker = datePicker;
		}

		/// <summary>
		/// Creates time picker in dialog window.
		/// </summary>
		public void InitializeTimePicker(TimeSpan time, string format)
		{
			var timePicker = new TimePicker(this)
			{
				Time = time,
				DateTimeFormat = format
			};
			Content = DateTimePicker = timePicker;
		}

		void ApplyDateTimePicker(DateTimeSelector dateTimePicker)
		{
			_dateTimePicker = dateTimePicker;
			Content = _dateTimePicker;
		}

		void Initialize()
		{
			//TODO need to add internationalization support
			PositiveButton = new EButton(_parent) { Text = "Set" };
			PositiveButton.Clicked += (s, e) =>
			{
				DateTime oldDate = DateTimePicker.DateTime;
				DateTimeChanged?.Invoke(this, new DateChangedEventArgs(oldDate, DateTimePicker.DateTime));
				Hide();
			};

			//TODO need to add internationalization support
			NegativeButton = new EButton(_parent) { Text = "Cancel" };
			NegativeButton.Clicked += (s, e) =>
			{
				Hide();
			};
			BackButtonPressed += (object s, EventArgs e) =>
			{
				Hide();
			};
		}
	}
}
