using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{
	public class DateEventArgs(DateTime date) : EventArgs
	{
		public DateTime Date { get; } = date;
	}

	public class GrabHelper
	{
		public static void GrabWindow(Window window)
		{
			window.GrabFocus();

			Grab.Add(window);
			Gdk.GrabStatus grabbed =
				Gdk.Display.Default.DefaultSeat.Grab(window.Window, Gdk.SeatCapabilities.All, true, null, null, null);
			if (grabbed != Gdk.GrabStatus.Success)
			{
				Grab.Remove(window);
				window.Destroy();
			}
		}

		public static void RemoveGrab(Window window)
		{
			Grab.Remove(window);
			Gdk.Display.Default.DefaultSeat.Ungrab();
		}
	}

	public partial class DatePickerWindow : Window
	{
		RangeCalendar _calendar;

		public delegate void DateEventHandler(object sender, DateEventArgs args);

		public event DateEventHandler? OnDateTimeChanged;

		public DatePickerWindow(DateTime initialDate, Window? parentWindow)
			: base(WindowType.Popup)
		{
			if (parentWindow is not null)
				TransientFor = parentWindow;
			Title = "DatePicker";
			TypeHint = Gdk.WindowTypeHint.Desktop;
			WindowPosition = WindowPosition.None;
			BorderWidth = 1;
			Resizable = false;
			Decorated = false;
			DestroyWithParent = true;
			SkipPagerHint = true;
			SkipTaskbarHint = true;
			var datebox = new Box(Orientation.Vertical, 6);
			datebox.BorderWidth = 3;

			_calendar = new RangeCalendar();
			_calendar.CanFocus = true;
			_calendar.DisplayOptions = CalendarDisplayOptions.ShowHeading;
			datebox.Add(_calendar);
			Box.BoxChild dateBoxChild = ((Box.BoxChild)(datebox[_calendar]));
			dateBoxChild.Position = 0;

			Add(datebox);

			Child?.ShowAll();

			SelectedDate = initialDate;
			Show();
			ButtonPressEvent += new ButtonPressEventHandler(OnButtonPressEvent);
			_calendar.ButtonPressEvent += new ButtonPressEventHandler(OnCalendarButtonPressEvent);
			_calendar.DaySelected += new EventHandler(OnCalendarDaySelected);
			_calendar.DaySelectedDoubleClick += new EventHandler(OnCalendarDaySelectedDoubleClick);
			GrabHelper.GrabWindow(this);
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

		public void ShowCalendar()
		{
			Child?.ShowAll();

			Show();
			GrabHelper.GrabWindow(this);
		}

		void CloseCalendar()
		{
			GrabHelper.RemoveGrab(this);
			Hide();
		}

		void NotifyDateChanged()
		{
			OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
		}

		class RangeCalendar : Calendar
		{
			DateTime _minimumDate = new(1900, 1, 1);
			DateTime _maximumDate = new(2199, 1, 1);

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

	public class CustomComboBox : Box
	{
		Color _color = Colors.Black;

		public CustomComboBox() : base(Orientation.Horizontal, 0)
		{
			Entry = new Entry();
			Entry.CanFocus = true;
			Entry.IsEditable = true;
			Entry.SetIconFromIconName(EntryIconPosition.Secondary, "pan-down-symbolic");
			PackStart(Entry, true, true, 0);
		}

		public Entry Entry { get; }

		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;
				Entry.UpdateTextColor(_color);
			}
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

			Child?.ShowAll();
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
			_comboBox.Entry.IconPress += new IconPressHandler(OnBtnShowCalendarClicked);
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

		void ShowPickerWindow()
		{
			Window.GetOrigin(out var x, out var y);
			y += Allocation.Height;

			if (_pickerWindow is not { })
			{
				var picker = new DatePickerWindow(Date, Toplevel as Window);
				picker.Move(x, y);
				picker.MinimumDate = MinDate;
				picker.MaximumDate = MaxDate;
				picker.OnDateTimeChanged += OnPopupDateChanged;
				picker.Destroyed += OnPickerClosed;
				_pickerWindow = picker;
			}
			else
			{
				if (_pickerWindow.SelectedDate != Date)
					_pickerWindow.SelectedDate = Date;
				_pickerWindow.MinimumDate = MinDate;
				_pickerWindow.MaximumDate = MaxDate;
				_pickerWindow.ShowCalendar();
				_pickerWindow.Move(x, y);
			}
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
			if (sender is not DatePickerWindow window)
			{
				return;
			}

			window.Hide();
			LostFocus?.Invoke(this, EventArgs.Empty);
		}
	}
}