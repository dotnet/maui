using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Gtk.Hosting;
using Microsoft.Maui.Gtk.Platform;
using Microsoft.Maui.Handlers;
using GtkDropDown = Gtk.DropDown;
using GtkLabel = Gtk.Label;
using GtkMenuButton = Gtk.MenuButton;
using GtkPopover = Gtk.Popover;

namespace Microsoft.Maui.Gtk.Handlers
{
	/// <summary>
	/// A <c>Gtk.DropDown</c> over a <c>Gtk.StringList</c> with a title that is shown while nothing is selected (a drop-down has no
	/// placeholder of its own, so the title is a non-interactive label overlaid on it).
	/// </summary>
	public sealed class MauiGtkPicker : global::Gtk.Overlay
	{
		/// <summary>The value GTK uses for "no selection" (<c>GTK_INVALID_LIST_POSITION</c>).</summary>
		public const uint InvalidPosition = uint.MaxValue;

		readonly global::Gtk.StringList _items = new(Array.Empty<string>());
		readonly GtkDropDown _dropDown;
		readonly GtkLabel _title = new() { Xalign = 0, CanTarget = false, Halign = global::Gtk.Align.Start, Valign = global::Gtk.Align.Center, MarginStart = 10 };

		/// <summary>Creates the picker.</summary>
		public MauiGtkPicker()
		{
			_dropDown = new GtkDropDown(_items, null) { Hexpand = true };
			_title.AddCssClass("dim-label");
			Child = _dropDown;
			AddOverlay(_title);
			_dropDown.AddNotification("selected", (o, a) =>
			{
				UpdateTitleVisibility();
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			});
			UpdateTitleVisibility();
		}

		/// <summary>Raised when the selected index changed (programmatically or by the user).</summary>
		public event EventHandler? SelectionChanged;

		/// <summary>The drop-down widget.</summary>
		public GtkDropDown DropDown => _dropDown;

		/// <summary>The title label shown while nothing is selected.</summary>
		public GtkLabel TitleLabel => _title;

		/// <summary>The popover the drop-down opens, or <see langword="null"/> when GTK has not created it yet.</summary>
		public GtkPopover? Popover
		{
			get
			{
				for (var child = _dropDown.FirstChild; child is not null; child = child.NextSibling)
				{
					if (child is GtkPopover popover)
						return popover;
				}

				return null;
			}
		}

		/// <summary>The title text.</summary>
		public string Title
		{
			get => _title.Text ?? string.Empty;
			set => _title.Text = value ?? string.Empty;
		}

		/// <summary>The number of items.</summary>
		public int ItemCount => (int)_items.NItems;

		/// <summary>The selected index, or -1.</summary>
		public int SelectedIndex
		{
			get => _dropDown.Selected == InvalidPosition ? -1 : (int)_dropDown.Selected;
			set
			{
				uint target = value < 0 || value >= ItemCount ? InvalidPosition : (uint)value;
				if (_dropDown.Selected != target)
					_dropDown.Selected = target;
				UpdateTitleVisibility();
			}
		}

		/// <summary>Replaces the items. The selection is cleared by GTK when it no longer fits.</summary>
		/// <param name="items">The item texts.</param>
		public void SetItems(IList<string>? items)
		{
			var array = new string[items?.Count ?? 0];
			for (int i = 0; i < array.Length; i++)
				array[i] = items![i] ?? string.Empty;

			_items.Splice(0, _items.NItems, array);
			UpdateTitleVisibility();
		}

		/// <summary>Gets the text of the item at <paramref name="index"/>.</summary>
		/// <param name="index">The index.</param>
		public string GetItem(int index) => _items.GetString((uint)index) ?? string.Empty;

		void UpdateTitleVisibility() => _title.Visible = _dropDown.Selected == InvalidPosition && !string.IsNullOrEmpty(_title.Text);
	}

	/// <summary>
	/// <see cref="PickerHandler"/> over a <see cref="MauiGtkPicker"/> (<c>Gtk.DropDown</c> + <c>Gtk.StringList</c>).
	/// </summary>
	/// <remarks>
	/// Text colour, font and letter spacing are CSS on the picker (they cascade to the drop-down and its list); vertical text alignment has no
	/// GTK equivalent. <c>IsOpen</c> activates the drop-down; it is not exercised on Windows, where <c>Popover.Popup</c> crashes with the gvsbuild runtime.
	/// </remarks>
	public class GtkPickerHandler : PickerHandler
	{
		readonly GtkUpdateGuard _guard = new();
		GtkPopover? _popover;

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkPickerHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkPickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkPicker { Halign = global::Gtk.Align.Fill, Valign = global::Gtk.Align.Center };

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is MauiGtkPicker picker)
				picker.SelectionChanged += OnSelectionChanged;
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkPicker picker)
				picker.SelectionChanged -= OnSelectionChanged;

			if (_popover is not null)
			{
				_popover.Closed -= OnPopoverClosed;
				_popover = null;
			}

			base.DisconnectHandler(platformView);
		}

		void OnSelectionChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not MauiGtkPicker picker || ((IElementHandler)this).VirtualView is not IPicker view)
				return;

			if (view.SelectedIndex != picker.SelectedIndex)
				view.SelectedIndex = picker.SelectedIndex;
		}

		void OnPopoverClosed(object? sender, EventArgs e)
		{
			if (((IElementHandler)this).VirtualView is IPicker { IsOpen: true } view)
				view.IsOpen = false;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkPickerHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		static void UpdateItems(IPickerHandler handler, IPicker view, MauiGtkPicker picker) => Guarded(handler, () =>
		{
			picker.SetItems(view.Items);
			picker.SelectedIndex = view.SelectedIndex;
		});

		static void UpdateIsOpen(IPickerHandler handler, IPicker view, MauiGtkPicker picker)
		{
			if (view.IsOpen)
			{
				((global::Gtk.Widget)picker.DropDown).Activate();

				if (handler is GtkPickerHandler gtk && gtk._popover is null && picker.Popover is { } popover)
				{
					gtk._popover = popover;
					popover.Closed += gtk.OnPopoverClosed;
				}
			}
			else
			{
				picker.Popover?.Popdown();
			}
		}

		internal static readonly IPropertyMapper<IPicker, IPickerHandler> GtkMapper =
			new GtkMapperBuilder<IPicker, IPickerHandler>(PickerHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkPicker>(nameof(IPicker.Items), (h, v, w) => UpdateItems(h, v, w))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.SelectedIndex), (h, v, w) => Guarded(h, () => w.SelectedIndex = v.SelectedIndex))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.Title), (h, v, w) => w.Title = v.Title ?? string.Empty)
				.MapGtk<MauiGtkPicker>(nameof(IPicker.TitleColor), (h, v, w) => GtkTextStyle.SetColor(w.TitleLabel, v.TitleColor))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<MauiGtkPicker>(nameof(ITextAlignment.HorizontalTextAlignment), (h, v, w) =>
					w.TitleLabel.Xalign = GtkTextStyle.ToAlignment(v.HorizontalTextAlignment, v.FlowDirection))
				.MapGtk<MauiGtkPicker>(nameof(ITextAlignment.VerticalTextAlignment), (h, v, w) => { /* a drop-down always centres its text vertically */ })
				.MapGtk<MauiGtkPicker>(nameof(IPicker.IsOpen), (h, v, w) => UpdateIsOpen(h, v, w))
				.Build();

		internal static readonly CommandMapper<IPicker, IPickerHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IPicker, IPickerHandler>(PickerHandler.CommandMapper).AddViewCommands().Build();
	}

	/// <summary>
	/// The shared part of the date and time pickers: a <c>Gtk.MenuButton</c> whose label is the formatted value and whose popover holds the editor.
	/// </summary>
	public abstract class MauiGtkPopupPicker : GtkMenuButton
	{
		/// <summary>The popover with the editor.</summary>
		protected GtkPopover EditorPopover { get; } = new();

		/// <summary>Creates the button.</summary>
		protected MauiGtkPopupPicker()
		{
			Popover = EditorPopover;
			Halign = global::Gtk.Align.Fill;
			Valign = global::Gtk.Align.Center;
			Hexpand = true;
			AlwaysShowArrow = true;
		}

		/// <summary>The popover with the editor.</summary>
		public GtkPopover Editor => EditorPopover;

		/// <summary>Formats with <paramref name="format"/>, falling back to the short pattern when the format is invalid.</summary>
		/// <param name="value">The value.</param>
		/// <param name="format">A .NET date/time format string.</param>
		protected static string FormatValue(DateTime value, string? format)
		{
			try
			{
				return value.ToString(string.IsNullOrEmpty(format) ? "d" : format, CultureInfo.CurrentCulture);
			}
			catch (FormatException)
			{
				return value.ToString("d", CultureInfo.CurrentCulture);
			}
		}
	}

	/// <summary>A date picker: a menu button whose popover holds a <c>Gtk.Calendar</c>.</summary>
	public sealed class MauiGtkDatePicker : MauiGtkPopupPicker
	{
		readonly global::Gtk.Calendar _calendar = new();
		DateTime? _date;
		DateTime? _minimum;
		DateTime? _maximum;
		string _format = "d";
		bool _updating;

		/// <summary>Creates the picker.</summary>
		public MauiGtkDatePicker()
		{
			EditorPopover.Child = _calendar;
			_calendar.DaySelected += OnDaySelected;
		}

		/// <summary>Raised when the user (or <see cref="Date"/>) changed the date.</summary>
		public event EventHandler? DateChanged;

		/// <summary>The calendar widget.</summary>
		public global::Gtk.Calendar Calendar => _calendar;

		/// <summary>The display format.</summary>
		public string Format
		{
			get => _format;
			set
			{
				_format = value;
				UpdateLabel();
			}
		}

		/// <summary>The earliest selectable date.</summary>
		public DateTime? MinimumDate
		{
			get => _minimum;
			set
			{
				_minimum = value;
				Date = _date;
			}
		}

		/// <summary>The latest selectable date.</summary>
		public DateTime? MaximumDate
		{
			get => _maximum;
			set
			{
				_maximum = value;
				Date = _date;
			}
		}

		/// <summary>The selected date, clamped to the minimum and maximum; <see langword="null"/> shows an empty label.</summary>
		public DateTime? Date
		{
			get => _date;
			set
			{
				var clamped = Clamp(value);
				bool changed = clamped != _date;
				_date = clamped;

				if (clamped is { } date)
				{
					_updating = true;
					try
					{
#pragma warning disable CS0612 // Calendar.SelectDay(GLib.DateTime) is flagged obsolete by the binding but is the only way to set the selection.
						_calendar.SelectDay(GLib.DateTime.FromSystemDateTime(date));
#pragma warning restore CS0612
					}
					finally
					{
						_updating = false;
					}
				}

				UpdateLabel();
				if (changed)
					DateChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		DateTime? Clamp(DateTime? value)
		{
			if (value is not { } date)
				return null;
			if (_minimum is { } min && date < min)
				return min.Date;
			if (_maximum is { } max && date > max)
				return max.Date;
			return date.Date;
		}

		void OnDaySelected(object? sender, EventArgs e)
		{
			if (_updating)
				return;

			Date = _calendar.Date.ToSystemDateTime();
		}

		void UpdateLabel() => Label = _date is { } date ? FormatValue(date, _format) : string.Empty;
	}

	/// <summary>
	/// <see cref="DatePickerHandler"/> over a <see cref="MauiGtkDatePicker"/> (<c>Gtk.MenuButton</c> + <c>Gtk.Popover</c> + <c>Gtk.Calendar</c>).
	/// </summary>
	/// <remarks><c>IsOpen</c> pops the popover up, which is not exercised on Windows (<c>Popover.Popup</c> crashes with the gvsbuild runtime).</remarks>
	public class GtkDatePickerHandler : DatePickerHandler
	{
		readonly GtkUpdateGuard _guard = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkDatePickerHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkDatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkDatePicker();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is MauiGtkDatePicker picker)
			{
				picker.DateChanged += OnDateChanged;
				picker.AddNotification("active", OnActiveChanged);
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkDatePicker picker)
				picker.DateChanged -= OnDateChanged;

			base.DisconnectHandler(platformView);
		}

		void OnDateChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not MauiGtkDatePicker picker || ((IElementHandler)this).VirtualView is not IDatePicker view)
				return;

			if (view.Date != picker.Date)
				view.Date = picker.Date;
		}

		void OnActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (_guard.IsActive || o is not MauiGtkDatePicker picker || ((IElementHandler)this).VirtualView is not IDatePicker view)
				return;

			if (view.IsOpen != picker.Active)
				view.IsOpen = picker.Active;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkDatePickerHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		internal static readonly IPropertyMapper<IDatePicker, IDatePickerHandler> GtkMapper =
			new GtkMapperBuilder<IDatePicker, IDatePickerHandler>(DatePickerHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.Format), (h, v, w) => Guarded(h, () => w.Format = v.Format))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.MinimumDate), (h, v, w) => Guarded(h, () => w.MinimumDate = v.MinimumDate))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.MaximumDate), (h, v, w) => Guarded(h, () => w.MaximumDate = v.MaximumDate))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.Date), (h, v, w) => Guarded(h, () => w.Date = v.Date))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<MauiGtkDatePicker>(nameof(IDatePicker.IsOpen), (h, v, w) => Guarded(h, () =>
				{
					if (w.Active != v.IsOpen)
						w.Active = v.IsOpen;
				}))
				.Build();

		internal static readonly CommandMapper<IDatePicker, IDatePickerHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<IDatePicker, IDatePickerHandler>(new CommandMapper<IDatePicker, IDatePickerHandler>(ViewCommandMapper)).AddViewCommands().Build();
	}

	/// <summary>A time picker: a menu button whose popover holds hour and minute spin buttons.</summary>
	public sealed class MauiGtkTimePicker : MauiGtkPopupPicker
	{
		readonly global::Gtk.SpinButton _hours = new(0, 23, 1) { Wrap = true, Orientation = global::Gtk.Orientation.Vertical };
		readonly global::Gtk.SpinButton _minutes = new(0, 59, 1) { Wrap = true, Orientation = global::Gtk.Orientation.Vertical };
		TimeSpan? _time;
		string _format = "t";
		bool _updating;

		/// <summary>Creates the picker.</summary>
		public MauiGtkTimePicker()
		{
			var box = new global::Gtk.Box(global::Gtk.Orientation.Horizontal, 6) { MarginTop = 6, MarginBottom = 6, MarginStart = 6, MarginEnd = 6 };
			box.Append(_hours);
			box.Append(new global::Gtk.Label(":"));
			box.Append(_minutes);
			EditorPopover.Child = box;
			_hours.ValueChanged += OnSpinChanged;
			_minutes.ValueChanged += OnSpinChanged;
		}

		/// <summary>Raised when the user (or <see cref="Time"/>) changed the time.</summary>
		public event EventHandler? TimeChanged;

		/// <summary>The hour spin button.</summary>
		public global::Gtk.SpinButton Hours => _hours;

		/// <summary>The minute spin button.</summary>
		public global::Gtk.SpinButton Minutes => _minutes;

		/// <summary>The display format (a .NET date/time format applied to the time of day).</summary>
		public string Format
		{
			get => _format;
			set
			{
				_format = value;
				UpdateLabel();
			}
		}

		/// <summary>The selected time of day; <see langword="null"/> shows an empty label.</summary>
		public TimeSpan? Time
		{
			get => _time;
			set
			{
				var normalized = value is { } time ? new TimeSpan(time.Hours, time.Minutes, 0) : (TimeSpan?)null;
				bool changed = normalized != _time;
				_time = normalized;

				if (normalized is { } t)
				{
					_updating = true;
					try
					{
						_hours.Value = t.Hours;
						_minutes.Value = t.Minutes;
					}
					finally
					{
						_updating = false;
					}
				}

				UpdateLabel();
				if (changed)
					TimeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		void OnSpinChanged(object? sender, EventArgs e)
		{
			if (_updating)
				return;

			Time = new TimeSpan(_hours.ValueAsInt, _minutes.ValueAsInt, 0);
		}

		void UpdateLabel() => Label = _time is { } time ? FormatValue(DateTime.Today.Add(time), _format) : string.Empty;
	}

	/// <summary>
	/// <see cref="TimePickerHandler"/> over a <see cref="MauiGtkTimePicker"/> (<c>Gtk.MenuButton</c> + <c>Gtk.Popover</c> + two <c>Gtk.SpinButton</c>s).
	/// </summary>
	/// <remarks><c>IsOpen</c> pops the popover up, which is not exercised on Windows (<c>Popover.Popup</c> crashes with the gvsbuild runtime).</remarks>
	public class GtkTimePickerHandler : TimePickerHandler
	{
		readonly GtkUpdateGuard _guard = new();

		/// <summary>Creates the handler with the stock mappers.</summary>
		public GtkTimePickerHandler()
			: base(GtkMapper, GtkCommandMapper)
		{
		}

		/// <summary>Creates the handler with custom mappers.</summary>
		/// <param name="mapper">The property mapper, or <see langword="null"/> for the stock one.</param>
		/// <param name="commandMapper">The command mapper, or <see langword="null"/> for the stock one.</param>
		public GtkTimePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper = null)
			: base(mapper ?? GtkMapper, commandMapper ?? GtkCommandMapper)
		{
		}

		/// <inheritdoc/>
		protected override object CreatePlatformView() => new MauiGtkTimePicker();

		/// <inheritdoc/>
		protected override void ConnectHandler(object platformView)
		{
			base.ConnectHandler(platformView);
			if (platformView is MauiGtkTimePicker picker)
			{
				picker.TimeChanged += OnTimeChanged;
				picker.AddNotification("active", OnActiveChanged);
			}
		}

		/// <inheritdoc/>
		protected override void DisconnectHandler(object platformView)
		{
			if (platformView is MauiGtkTimePicker picker)
				picker.TimeChanged -= OnTimeChanged;

			base.DisconnectHandler(platformView);
		}

		void OnTimeChanged(object? sender, EventArgs e)
		{
			if (_guard.IsActive || sender is not MauiGtkTimePicker picker || ((IElementHandler)this).VirtualView is not ITimePicker view)
				return;

			if (view.Time != picker.Time)
				view.Time = picker.Time;
		}

		void OnActiveChanged(object o, GLib.NotifyArgs args)
		{
			if (_guard.IsActive || o is not MauiGtkTimePicker picker || ((IElementHandler)this).VirtualView is not ITimePicker view)
				return;

			if (view.IsOpen != picker.Active)
				view.IsOpen = picker.Active;
		}

		static void Guarded(IViewHandler handler, Action action)
		{
			if (handler is GtkTimePickerHandler gtk)
				gtk._guard.Run(action);
			else
				action();
		}

		internal static readonly IPropertyMapper<ITimePicker, ITimePickerHandler> GtkMapper =
			new GtkMapperBuilder<ITimePicker, ITimePickerHandler>(TimePickerHandler.Mapper)
				.AddViewMappings()
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.Format), (h, v, w) => Guarded(h, () => w.Format = v.Format))
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.Time), (h, v, w) => Guarded(h, () => w.Time = v.Time))
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.TextColor), (h, v, w) => GtkTextStyle.SetColor(w, v.TextColor))
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<MauiGtkTimePicker>(nameof(ITimePicker.IsOpen), (h, v, w) => Guarded(h, () =>
				{
					if (w.Active != v.IsOpen)
						w.Active = v.IsOpen;
				}))
				.Build();

		internal static readonly CommandMapper<ITimePicker, ITimePickerHandler> GtkCommandMapper =
			new GtkCommandMapperBuilder<ITimePicker, ITimePickerHandler>(new CommandMapper<ITimePicker, ITimePickerHandler>(ViewCommandMapper)).AddViewCommands().Build();
	}
}
