using System;
using System.Runtime.Serialization;
using System.Linq;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Platform
{
	public class DateEventArgs : EventArgs
	{
		DateTime _date;

		public DateTime Date
		{
			get
			{
				return _date;
			}
		}

		public DateEventArgs(DateTime date)
		{
			_date = date;
		}
	}

	public class GrabHelper
	{
		private static uint CURRENT_TIME = 0;

		public static void GrabWindow(Window window)
		{
			window.GrabFocus();

			Grab.Add(window);
			// error CS0612: 'Pointer.Grab(Window, bool, EventMask, Window, Cursor, uint)' is obsolete
#pragma warning disable 612
			Gdk.GrabStatus grabbed =
				Gdk.Pointer.Grab(window.Window, true,
				Gdk.EventMask.ButtonPressMask
				| Gdk.EventMask.ButtonReleaseMask
				| Gdk.EventMask.PointerMotionMask, null, null, CURRENT_TIME);
#pragma warning restore 612
			if (grabbed == Gdk.GrabStatus.Success)
			{
				// error CS0612: 'Keyboard.Grab(Window, bool, uint)' is obsolete
#pragma warning disable 612
				grabbed = Gdk.Keyboard.Grab(window.Window, true, CURRENT_TIME);
#pragma warning restore 612
				if (grabbed != Gdk.GrabStatus.Success)
				{
					Grab.Remove(window);
					window.Destroy();
				}
			}
			else
			{
				Grab.Remove(window);
				window.Destroy();
			}
		}

		public static void RemoveGrab(Window window)
		{
			Grab.Remove(window);
			// error CS0612: 'Pointer.Ungrab(uint)' is obsolete && error CS0612: 'Keyboard.Ungrab(uint)' is obsolete
#pragma warning disable 612
			Gdk.Pointer.Ungrab(CURRENT_TIME);
			Gdk.Keyboard.Ungrab(CURRENT_TIME);
#pragma warning restore 612
		}
	}

	public partial class DatePickerWindow : Window
	{
		VBox _datebox;
		RangeCalendar _calendar;
		public delegate void DateEventHandler(object sender, DateEventArgs args);

		public event DateEventHandler? OnDateTimeChanged;

		public DatePickerWindow()
			: base(WindowType.Popup)
		{
			Title = "DatePicker";
			TypeHint = Gdk.WindowTypeHint.Desktop;
			WindowPosition = WindowPosition.Mouse;
			BorderWidth = 1;
			Resizable = false;
			Decorated = false;
			DestroyWithParent = true;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			// error CS0612: 'VBox.VBox()' is obsolete
#pragma warning disable 612
			_datebox = new VBox();
#pragma warning restore 612
			_datebox.Spacing = 6;
			_datebox.BorderWidth = 3;

			_calendar = new RangeCalendar();
			_calendar.CanFocus = true;
			_calendar.DisplayOptions = CalendarDisplayOptions.ShowHeading;
			_datebox.Add(_calendar);
			Box.BoxChild dateBoxChild = ((Box.BoxChild)(_datebox[_calendar]));
			dateBoxChild.Position = 0;

			Add(_datebox);

			if ((Child != null))
			{
				Child.ShowAll();
			}

			Show();
			ButtonPressEvent += new ButtonPressEventHandler(OnButtonPressEvent);
			_calendar.ButtonPressEvent += new ButtonPressEventHandler(OnCalendarButtonPressEvent);
			_calendar.DaySelected += new EventHandler(OnCalendarDaySelected);
			_calendar.DaySelectedDoubleClick += new EventHandler(OnCalendarDaySelectedDoubleClick);
			GrabHelper.GrabWindow(this);
			SelectedDate = DateTime.Now;
		}

		public DateTime SelectedDate
		{
			get
			{
				return _calendar.Date;
			}

			set
			{
				_calendar.Date = value;
			}
		}

		public DateTime MinimumDate
		{
			get
			{
				return _calendar.MinimumDate;
			}

			set
			{
				_calendar.MinimumDate = value;
			}
		}

		public DateTime MaximumDate
		{
			get
			{
				return _calendar.MaximumDate;
			}

			set
			{
				_calendar.MaximumDate = value;
			}
		}

		protected virtual void OnButtonPressEvent(object sender, ButtonPressEventArgs args)
		{
			CloseCalendar();
		}

		protected virtual void OnCalendarButtonPressEvent(object sender, ButtonPressEventArgs args)
		{
			args.RetVal = true;
		}

		protected virtual void OnCalendarDaySelected(object? sender, EventArgs eventArgs)
		{
			OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
		}

		protected virtual void OnCalendarDaySelectedDoubleClick(object? sender, EventArgs eventArgs)
		{
			OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
			CloseCalendar();
		}

		void CloseCalendar()
		{
			GrabHelper.RemoveGrab(this);
			Destroy();
		}

		void NotifyDateChanged()
		{
			OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
		}

		class RangeCalendar : Calendar
		{
			DateTime _minimumDate;
			DateTime _maximumDate;

			public RangeCalendar()
			{
				_minimumDate = new DateTime(1900, 1, 1);
				_maximumDate = new DateTime(2199, 1, 1);
			}

			public DateTime MinimumDate
			{
				get
				{
					return _minimumDate;
				}

				set
				{
					if (MaximumDate < value)
					{
						throw new InvalidOperationException($"{nameof(MinimumDate)} must be lower than {nameof(MaximumDate)}");
					}

					_minimumDate = value;
				}
			}

			public DateTime MaximumDate
			{
				get
				{
					return _maximumDate;
				}

				set
				{
					if (MinimumDate > value)
					{
						throw new InvalidOperationException($"{nameof(MaximumDate)} must be greater than {nameof(MinimumDate)}");
					}

					_maximumDate = value;
				}
			}

			protected override void OnDaySelected()
			{
				if (Date < MinimumDate)
				{
					Date = MinimumDate;
				}
				else if (Date > MaximumDate)
				{
					Date = MaximumDate;
				}
			}
		}
	}

	public class CustomComboBox : Gtk.HBox
	{
		private Gtk.Entry _entry;
		private Gtk.Button _button;
		private Gtk.Arrow _arrow;
		private Color _color = Colors.Black;

		// error CS0612: 'HBox.HBox()' is obsolete 
#pragma warning disable 612
		public CustomComboBox()
		{
#pragma warning restore 612
			_entry = new Gtk.Entry();
			_entry.CanFocus = true;
			_entry.IsEditable = true;
			PackStart(_entry, true, true, 0);

			_button = new Gtk.Button();
			_button.WidthRequest = 30;
			_button.CanFocus = true;
			// error CS0612: 'Arrow.Arrow(ArrowType, ShadowType)' is obsolete
#pragma warning disable 612
			_arrow = new Gtk.Arrow(Gtk.ArrowType.Down, Gtk.ShadowType.EtchedOut);
#pragma warning restore 612
			_button.Add(_arrow);
			PackEnd(_button, false, false, 0);
		}

		public Gtk.Entry Entry
		{
			get
			{
				return _entry;
			}
		}

		public Gtk.Button PopupButton
		{
			get
			{
				return _button;
			}
		}

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				Entry.UpdateTextColor(_color);
			}
		}

		public void SetBackgroundColor(Gdk.Color color)
		{
			// error CS0612: 'Widget.ModifyBg(StateType, Color)' is obsolete && error CS0612: 'Widget.OverrideBackgroundColor(StateFlags, RGBA)' is obsolete
#pragma warning disable 612
			ModifyBg(Gtk.StateType.Normal, Colors.Red.ToGdkColor());
			Entry.OverrideBackgroundColor(Gtk.StateFlags.Normal, Colors.Blue.ToGdkRgba());
#pragma warning restore 612
		}
	}

	public class MauiDatePicker : EventBox
	{
		readonly CustomComboBox _comboBox;
		DateTime _currentDate;
		string? _dateFormat;
		DatePickerWindow? _pickerWindow;

		public event EventHandler DateChanged = new EventHandler((sender, args) => { });
		public event EventHandler GotFocus = new EventHandler((sender, args) => { });
		public event EventHandler LostFocus = new EventHandler((sender, args) => { });

		public MauiDatePicker()
		{
			_comboBox = new CustomComboBox();
			Add(_comboBox);

			if ((Child != null))
				Child.ShowAll();
			Show();

			Date = DateTime.Now;
			Format = string.Empty;
			MinDate = new DateTime(1900, 1, 1);
			MaxDate = new DateTime(2199, 1, 1);

			_comboBox.Entry.CanDefault = false;
			_comboBox.Entry.CanFocus = false;
			_comboBox.Entry.IsEditable = false;
			_comboBox.Entry.SetStateFlags(StateFlags.Normal, true);
			_comboBox.Entry.CanFocus = false;
			_comboBox.Entry.IsEditable = false;
			_comboBox.Entry.FocusGrabbed += new EventHandler(OnEntryFocused);
			_comboBox.PopupButton.Clicked += new EventHandler(OnBtnShowCalendarClicked);
		}

		public DateTime Date
		{
			get
			{
				return _currentDate;
			}
			set
			{
				_currentDate = value;
				UpdateEntryText();
			}
		}

		public DateTime MinDate { get; set; }

		public DateTime MaxDate { get; set; }

		public string? Format
		{
			get
			{
				return _dateFormat;
			}
			set
			{
				_dateFormat = value;
				UpdateEntryText();
			}
		}

		public void SetBackgroundColor(Gdk.Color color)
		{
			_comboBox.SetBackgroundColor(color);
		}

		void ShowPickerWindow()
		{
			int x = 0;
			int y = 0;

			Window.GetOrigin(out x, out y);
			y += Allocation.Height;

			var picker = new DatePickerWindow();
			picker.Move(x, y);
			picker.SelectedDate = Date;
			picker.MinimumDate = MinDate;
			picker.MaximumDate = MaxDate;
			picker.OnDateTimeChanged += OnPopupDateChanged;
			picker.Destroyed += OnPickerClosed;

			_pickerWindow = picker;
		}

		void OnPopupDateChanged(object sender, DateEventArgs args)
		{
			var date = args.Date;

			if (date < MinDate)
			{
				Date = MinDate;
				return;
			}

			if (date > MaxDate)
			{
				Date = MaxDate;
				return;
			}

			Date = args.Date;
			DateChanged?.Invoke(this, EventArgs.Empty);
		}

		void UpdateEntryText()
		{
			_comboBox.Entry.Text = _currentDate.ToString(string.IsNullOrEmpty(_dateFormat) ? "D" : _dateFormat);
		}

		void OnBtnShowCalendarClicked(object? sender, EventArgs eventArgs)
		{
			ShowPickerWindow();
		}

		void OnEntryFocused(object? sender, EventArgs eventArgs)
		{
			ShowPickerWindow();
			GotFocus?.Invoke(this, EventArgs.Empty);
		}

		void OnPickerClosed(object? sender, EventArgs eventArgs)
		{
			if (_pickerWindow != null)
			{
				Remove(_pickerWindow);
				LostFocus?.Invoke(this, EventArgs.Empty);
				_pickerWindow = null;
			}
		}
	}
}