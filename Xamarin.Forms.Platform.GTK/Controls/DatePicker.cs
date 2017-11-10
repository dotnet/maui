using Gtk;
using System;
using System.Linq;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class DateEventArgs : EventArgs
    {
        private DateTime _date;

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

    public partial class DatePickerWindow : Window
    {
        private VBox _datebox;
        private RangeCalendar _calendar;

        public DatePickerWindow()
            : base(WindowType.Popup)
        {
            BuildDatePickerWindow();
            Helpers.GrabHelper.GrabWindow(this);
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

        public delegate void DateEventHandler(object sender, DateEventArgs args);

        public event DateEventHandler OnDateTimeChanged;

        protected override bool OnExposeEvent(Gdk.EventExpose args)
        {
            base.OnExposeEvent(args);

            int winWidth, winHeight;
            GetSize(out winWidth, out winHeight);
            GdkWindow.DrawRectangle(
                Style.ForegroundGC(StateType.Insensitive), false, 0, 0, winWidth - 1, winHeight - 1);

            return false;
        }

        protected virtual void OnButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            Close();
        }

        protected virtual void OnCalendarButtonPressEvent(object o, ButtonPressEventArgs args)
        {
            args.RetVal = true;
        }

        protected virtual void OnCalendarDaySelected(object sender, EventArgs e)
        {
            OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
        }

        protected virtual void OnCalendarDaySelectedDoubleClick(object sender, EventArgs e)
        {
            OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
            Close();
        }

        private void BuildDatePickerWindow()
        {
            Title = "DatePicker";
            TypeHint = Gdk.WindowTypeHint.Desktop;
            WindowPosition = WindowPosition.Mouse;
            BorderWidth = 1;
            Resizable = false;
            AllowGrow = false;
            Decorated = false;
            DestroyWithParent = true;
            SkipPagerHint = true;
            SkipTaskbarHint = true;

            _datebox = new VBox();
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
        }

        private void Close()
        {
            Helpers.GrabHelper.RemoveGrab(this);
            this.Dispose();
            this.Destroy();
        }

        private void NotifyDateChanged()
        {
            OnDateTimeChanged?.Invoke(this, new DateEventArgs(SelectedDate));
        }

        class RangeCalendar : Calendar
        {
            private DateTime _minimumDate;
            private DateTime _maximumDate;

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

                if (Date > MaximumDate)
                {
                    Date = MaximumDate;
                }
            }
        }
    }

    public partial class DatePicker : EventBox
    {
        private CustomComboBox _comboBox;
        private Gdk.Color _color;
        private DateTime _currentDate;
        private DateTime _minDate;
        private DateTime _maxDate;
        private string _dateFormat;

        public event EventHandler DateChanged;
        public event EventHandler GotFocus;
        public event EventHandler LostFocus;

        public DatePicker()
        {
            BuildDatePicker();

            CurrentDate = DateTime.Now;

            TextColor = _comboBox.Entry.Style.Text(StateType.Normal);

            _comboBox.Entry.CanDefault = false;
            _comboBox.Entry.CanFocus = false;
            _comboBox.Entry.IsEditable = false;
            _comboBox.Entry.State = StateType.Normal;
            _comboBox.Entry.FocusGrabbed += new EventHandler(OnEntryFocused);
            _comboBox.PopupButton.Clicked += new EventHandler(OnBtnShowCalendarClicked);
        }

        public DateTime CurrentDate
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

        public DateTime MinDate
        {
            get
            {
                return _minDate;
            }
            set
            {
                _minDate = value;
            }
        }

        public DateTime MaxDate
        {
            get
            {
                return _maxDate;
            }
            set
            {
                _maxDate = value;
            }
        }

        public Gdk.Color TextColor
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                _comboBox.Color = _color;
            }
        }

        public string DateFormat
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

        public void OpenPicker()
        {
            ShowPickerWindow();
        }

        public void ClosePicker()
        {
            var windows = Window.ListToplevels();
            var window = windows.FirstOrDefault(w => w.GetType() == typeof(DatePickerWindow));

            if (window != null)
            {
                Remove(window);
            }
        }

        private void ShowPickerWindow()
        {
            int x = 0;
            int y = 0;

            GdkWindow.GetOrigin(out x, out y);
            y += Allocation.Height;

            var picker = new DatePickerWindow();
            picker.Move(x, y);
            picker.SelectedDate = CurrentDate;
            picker.MinimumDate = _minDate;
            picker.MaximumDate = _maxDate;
            picker.OnDateTimeChanged += OnPopupDateChanged;
            picker.Destroyed += OnPickerClosed;
        }

        private void OnPopupDateChanged(object sender, DateEventArgs args)
        {
            var date = args.Date;

            if (date < MinDate)
            {
                CurrentDate = MinDate;
                return;
            }

            if (date > MaxDate)
            {
                CurrentDate = MaxDate;
                return;
            }

            CurrentDate = args.Date;
            DateChanged?.Invoke(this, EventArgs.Empty);
        }

        private void BuildDatePicker()
        {
            _comboBox = new CustomComboBox();
            Add(_comboBox);

            if ((Child != null))
            {
                Child.ShowAll();
            }

            Show();
        }

        private void UpdateEntryText()
        {
            _comboBox.Entry.Text = string.IsNullOrEmpty(_dateFormat)
                ? _currentDate.ToLongDateString()
                : _currentDate.ToString(_dateFormat);
        }

        private void OnBtnShowCalendarClicked(object sender, EventArgs e)
        {
            ShowPickerWindow();
        }

        private void OnEntryFocused(object sender, EventArgs e)
        {
            ShowPickerWindow();
            GotFocus?.Invoke(this, EventArgs.Empty);
        }

        private void OnPickerClosed(object sender, EventArgs e)
        {
            var window = sender as DatePickerWindow;

            if (window != null)
            {
                Remove(window);
                LostFocus?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}