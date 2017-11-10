using System;
using System.Linq;

namespace Xamarin.Forms.Platform.GTK.Controls
{
    public class TimeEventArgs : EventArgs
    {
        private TimeSpan _time;

        public TimeSpan Time
        {
            get
            {
                return _time;
            }
        }

        public TimeEventArgs(TimeSpan time)
        {
            _time = time;
        }
    }

    public class TimePickerWindow : Gtk.Window
    {
        private Gtk.HBox _timeBox;
        private Gtk.Label _labelHour;
        private Gtk.SpinButton _txtHour;
        private Gtk.Label _labelMin;
        private Gtk.SpinButton _txtMin;
        private Gtk.Label _labelSec;
        private Gtk.SpinButton _txtSec;

        public TimePickerWindow()
            : base(Gtk.WindowType.Popup)
        {
            BuildTimePickerWindow();
            Helpers.GrabHelper.GrabWindow(this);

            RefreshTime();
        }

        public TimeSpan CurrentTime
        {
            get
            {
                return new TimeSpan((int)_txtHour.Value, (int)_txtMin.Value, (int)_txtSec.Value);
            }

            set
            {
                _txtHour.Value = value.Hours;
                _txtMin.Value = value.Minutes;
                _txtSec.Value = value.Seconds;
            }
        }

        public delegate void TimeEventHandler(object sender, TimeEventArgs args);

        public event TimeEventHandler OnTimeChanged;

        protected override bool OnExposeEvent(Gdk.EventExpose args)
        {
            base.OnExposeEvent(args);

            int winWidth, winHeight;
            GetSize(out winWidth, out winHeight);
            GdkWindow.DrawRectangle(
                Style.ForegroundGC(Gtk.StateType.Insensitive), false, 0, 0, winWidth - 1, winHeight - 1);

            return false;
        }

        protected virtual void OnButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            Close();
        }

        private void BuildTimePickerWindow()
        {
            Title = "TimePicker";
            TypeHint = Gdk.WindowTypeHint.Normal;
            WindowPosition = Gtk.WindowPosition.None;
            BorderWidth = 1;
            Resizable = false;
            AllowGrow = false;
            Decorated = false;
            DestroyWithParent = true;
            SkipPagerHint = true;
            SkipTaskbarHint = true;

            _timeBox = new Gtk.HBox();
            _timeBox.Spacing = 6;
            _timeBox.BorderWidth = 3;

            _labelHour = new Gtk.Label();
            _labelHour.LabelProp = "H:";
            _timeBox.Add(_labelHour);

            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(_timeBox[_labelHour]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;

            _txtHour = new Gtk.SpinButton(0D, 24D, 1D);
            _txtHour.CanFocus = true;
            _txtHour.Adjustment.PageIncrement = 1D;
            _txtHour.ClimbRate = 1D;
            _txtHour.Numeric = true;
            _timeBox.Add(_txtHour);

            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(_timeBox[_txtHour]));
            w3.Position = 1;
            w3.Expand = false;
            w3.Fill = false;

            _labelMin = new Gtk.Label();
            _labelMin.LabelProp = "M:";
            _timeBox.Add(_labelMin);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(_timeBox[_labelMin]));

            w4.Position = 2;
            w4.Expand = false;
            w4.Fill = false;

            _txtMin = new Gtk.SpinButton(0D, 60D, 1D);
            _txtMin.CanFocus = true;
            _txtMin.Adjustment.PageIncrement = 10D;
            _txtMin.ClimbRate = 1D;
            _txtMin.Numeric = true;
            _timeBox.Add(_txtMin);

            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(_timeBox[_txtMin]));
            w5.Position = 3;
            w5.Expand = false;
            w5.Fill = false;

            _labelSec = new Gtk.Label();
            _labelSec.LabelProp = "S:";
            _timeBox.Add(_labelSec);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(_timeBox[_labelSec]));
            w6.Position = 4;
            w6.Expand = false;
            w6.Fill = false;

            _txtSec = new Gtk.SpinButton(0D, 60D, 1D);
            _txtSec.CanFocus = true;
            _txtSec.Adjustment.PageIncrement = 10D;
            _txtSec.ClimbRate = 1D;
            _txtSec.Numeric = true;
            _timeBox.Add(_txtSec);

            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(_timeBox[_txtSec]));
            w7.Position = 5;
            w7.Expand = false;
            w7.Fill = false;

            Add(_timeBox);

            if ((Child != null))
            {
                Child.ShowAll();
            }

            Show();

            ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnButtonPressEvent);
            _txtHour.ValueChanged += new EventHandler(OnTxtHourValueChanged);
            _txtHour.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtHourButtonPressEvent);
            _txtMin.ValueChanged += new EventHandler(OnTxtMinValueChanged);
            _txtMin.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtMinButtonPressEvent);
            _txtSec.ValueChanged += new EventHandler(OnTxtSecValueChanged);
            _txtSec.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTxtSecButtonPressEvent);
        }

        private void Close()
        {
            Helpers.GrabHelper.RemoveGrab(this);
            Destroy();
        }

        private void RefreshTime()
        {
            OnTimeChanged?.Invoke(this, new TimeEventArgs(CurrentTime));
        }

        protected virtual void OnTxtHourValueChanged(object sender, EventArgs e)
        {
            if (_txtHour.Value == 24)
                _txtHour.Value = 0;

            RefreshTime();
        }

        protected virtual void OnTxtMinValueChanged(object sender, EventArgs e)
        {
            if (_txtMin.Value == 60)
                _txtMin.Value = 0;

            RefreshTime();
        }

        protected virtual void OnTxtSecValueChanged(object sender, EventArgs e)
        {
            if (_txtSec.Value == 60)
                _txtSec.Value = 0;

            RefreshTime();
        }

        protected virtual void OnTxtHourButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            args.RetVal = true;
        }

        protected virtual void OnTxtMinButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            args.RetVal = true;
        }

        protected virtual void OnTxtSecButtonPressEvent(object o, Gtk.ButtonPressEventArgs args)
        {
            args.RetVal = true;
        }
    }

    public class TimePicker : Gtk.EventBox
    {
        private const string DefaultTimeFormat = @"hh\:mm\:ss";

        private CustomComboBox _comboBox;
        private Gdk.Color _color;
        private TimeSpan _currentTime;
        private string _timeFormat;

        public event EventHandler TimeChanged;
        public event EventHandler GotFocus;
        public event EventHandler LostFocus;

        public TimePicker()
        {
            BuildTimePicker();

            CurrentTime = new TimeSpan(DateTime.Now.Ticks);

            TextColor = _comboBox.Entry.Style.Text(Gtk.StateType.Normal);

            _comboBox.Entry.Changed += new EventHandler(OnTxtTimeChanged);
            _comboBox.PopupButton.Clicked += new EventHandler(OnBtnShowTimePickerClicked);
        }

        public TimeSpan CurrentTime
        {
            get
            {
                return _currentTime;
            }
            set
            {
                _currentTime = value;
                UpdateEntryText();
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

        public string TimeFormat
        {
            get
            {
                return _timeFormat;
            }
            set
            {
                _timeFormat = value;
                UpdateEntryText();
            }
        }

        public void SetBackgroundColor(Gdk.Color color)
        {
            _comboBox.SetBackgroundColor(color);
        }

        public void OpenPicker()
        {
            ShowTimePickerWindow();
        }

        public void ClosePicker()
        {
            var windows = Gtk.Window.ListToplevels();
            var window = windows.FirstOrDefault(w => w.GetType() == typeof(TimePickerWindow));

            if (window != null)
            {
                Remove(window);
            }
        }

        protected virtual void OnTxtTimeChanged(object sender, EventArgs e)
        {
            _comboBox.Entry.ModifyText(Gtk.StateType.Normal, TextColor);

            TimeChanged?.Invoke(this, e);
        }

        protected virtual void OnBtnShowTimePickerClicked(object sender, EventArgs e)
        {
            ShowTimePickerWindow();
        }

        private void ShowTimePickerWindow()
        {
            int x = 0;
            int y = 0;

            GdkWindow.GetOrigin(out x, out y);
            y += Allocation.Height;

            var picker = new TimePickerWindow();
            picker.Move(x, y);
            picker.CurrentTime = CurrentTime;
            picker.OnTimeChanged += OnPopupTimeChanged;
            picker.Destroyed += OnPickerClosed;
            GotFocus?.Invoke(this, EventArgs.Empty);
        }

        private void OnPopupTimeChanged(object sender, TimeEventArgs args)
        {
            CurrentTime = args.Time;
        }

        private void BuildTimePicker()
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
            _comboBox.Entry.Text = string.IsNullOrEmpty(_timeFormat)
                ? _currentTime.ToString(DefaultTimeFormat)
                : DateTime.Today.Date.Add(_currentTime).ToString(_timeFormat);
        }

        private void OnPickerClosed(object sender, EventArgs e)
        {
            var window = sender as TimePickerWindow;

            if (window != null)
            {
                Remove(window);
                LostFocus?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}