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
	/// A <c>Gtk.DropDown</c> over a <c>Gtk.StringList</c>. A drop-down cannot be left without a selection (its selection model always selects an item),
	/// so item 0 of the list is a placeholder row that shows the title: selecting it means "no selection" (<see cref="SelectedIndex"/> -1), and the
	/// real items start at position 1.
	/// </summary>
	public sealed class MauiGtkPicker : global::Gtk.Box
	{
		readonly global::Gtk.StringList _items = new(new[] { string.Empty });
		readonly GtkDropDown _dropDown;

		/// <summary>Creates the picker.</summary>
		public MauiGtkPicker()
			: base(global::Gtk.Orientation.Horizontal, 0)
		{
			_dropDown = new GtkDropDown(_items, null) { Hexpand = true };
			Append(_dropDown);
			_dropDown.AddNotification("selected", (o, a) =>
			{
				ApplyTextAlignment();
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			});
			_dropDown.AddNotification("root", (o, a) => ApplyTextAlignment());
			Popover?.AddNotification("visible", (o, a) => ApplyTextAlignment());
		}

		float _xalign;

		/// <summary>
		/// The horizontal position (0 start, 0.5 centre, 1 end) of the text of the drop-down button and of the rows of its popup list. The labels are
		/// created by the drop-down (the rows when the list is realised), so the value is re-applied on selection, rooting and whenever the popup opens.
		/// </summary>
		public float TextXAlign
		{
			get => _xalign;
			set
			{
				_xalign = value;
				ApplyTextAlignment();
			}
		}

		/// <summary>Applies <see cref="TextXAlign"/> to every label of the drop-down (button and popup rows).</summary>
		public void ApplyTextAlignment() => ApplyTextAlignment(_dropDown);

		void ApplyTextAlignment(global::Gtk.Widget parent)
		{
			for (var child = parent.FirstChild; child is not null; child = child.NextSibling)
			{
				if (child is global::Gtk.Label label)
				{
					label.Xalign = _xalign;
					label.Hexpand = true;
				}
				else
				{
					ApplyTextAlignment(child);
				}
			}
		}

		/// <summary>Raised when the selected index changed (programmatically or by the user).</summary>
		public event EventHandler? SelectionChanged;

		/// <summary>The drop-down widget.</summary>
		public GtkDropDown DropDown => _dropDown;

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

		/// <summary>The title: the text of the "no selection" row.</summary>
		public string Title
		{
			get => _items.GetString(0) ?? string.Empty;
			set
			{
				var text = value ?? string.Empty;
				if (_items.GetString(0) == text)
					return;

				// Replacing the row can move the selection; put it back.
				uint selected = _dropDown.Selected;
				_items.Splice(0, 1, new[] { text });
				if (_dropDown.Selected != selected)
					_dropDown.Selected = selected;
			}
		}

		/// <summary>The number of items (the placeholder row is not counted).</summary>
		public int ItemCount => (int)_items.NItems - 1;

		/// <summary>The selected index, or -1 for none (the placeholder row).</summary>
		public int SelectedIndex
		{
			get => _dropDown.Selected == 0 || _dropDown.Selected >= _items.NItems ? -1 : (int)_dropDown.Selected - 1;
			set
			{
				uint target = value < 0 || value >= ItemCount ? 0 : (uint)value + 1;
				if (_dropDown.Selected != target)
					_dropDown.Selected = target;
			}
		}

		/// <summary>Replaces the items. The selection falls back to the placeholder row when it no longer fits.</summary>
		/// <param name="items">The item texts.</param>
		public void SetItems(IList<string>? items)
		{
			var array = new string[items?.Count ?? 0];
			for (int i = 0; i < array.Length; i++)
				array[i] = items![i] ?? string.Empty;

			_items.Splice(1, _items.NItems - 1, array);
		}

		/// <summary>Gets the text of the item at <paramref name="index"/>.</summary>
		/// <param name="index">The index.</param>
		public string GetItem(int index) => _items.GetString((uint)index + 1) ?? string.Empty;
	}

	/// <summary>
	/// <see cref="PickerHandler"/> over a <see cref="MauiGtkPicker"/> (<c>Gtk.DropDown</c> + <c>Gtk.StringList</c>).
	/// </summary>
	/// <remarks>
	/// Text colour, font and letter spacing are CSS on the picker (they cascade to the drop-down and its list); the horizontal text alignment sets the
	/// <c>xalign</c> of the drop-down labels (button and list rows); the vertical one has no GTK equivalent (always centred). <c>IsOpen</c> pops the drop-down's popover up.
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

		// Nothing selected shows the title, in the title colour (when it has one).
		static void UpdateColor(MauiGtkPicker picker, IPicker view) =>
			GtkTextStyle.SetColor(picker, view.SelectedIndex < 0 ? view.TitleColor ?? view.TextColor : view.TextColor);

		static void UpdateItems(IPickerHandler handler, IPicker view, MauiGtkPicker picker) => Guarded(handler, () =>
		{
			picker.Title = view.Title ?? string.Empty;
			picker.SetItems(view.Items);
			picker.SelectedIndex = view.SelectedIndex;
		});

		static void UpdateIsOpen(IPickerHandler handler, IPicker view, MauiGtkPicker picker)
		{
			if (view.IsOpen)
			{
				// GtkDropDown owns its popover as a direct child: pop it up as the toggle button would.
				if (picker.Popover is { } dropDownPopover)
					dropDownPopover.Popup();
				else
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
				.MapGtk<MauiGtkPicker>(nameof(IPicker.SelectedIndex), (h, v, w) =>
				{
					Guarded(h, () => w.SelectedIndex = v.SelectedIndex);
					UpdateColor(w, v);
				})
				.MapGtk<MauiGtkPicker>(nameof(IPicker.Title), (h, v, w) => Guarded(h, () => w.Title = v.Title ?? string.Empty))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.TitleColor), (h, v, w) => UpdateColor(w, v))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.TextColor), (h, v, w) => UpdateColor(w, v))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.Font), (h, v, w) => GtkTextStyle.SetFont(w, v.Font, GtkTextStyle.GetFontManager(h)))
				.MapGtk<MauiGtkPicker>(nameof(IPicker.CharacterSpacing), (h, v, w) => GtkTextStyle.SetCharacterSpacing(w, v.CharacterSpacing))
				.MapGtk<MauiGtkPicker>(nameof(ITextAlignment.HorizontalTextAlignment), (h, v, w) => w.TextXAlign = GtkTextStyle.ToAlignment(v.HorizontalTextAlignment, v.FlowDirection))
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
	/// <remarks><c>IsOpen</c> pops the popover up.</remarks>
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
			{
				picker.DateChanged -= OnDateChanged;
				picker.RemoveNotification("active", OnActiveChanged);
			}

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
	/// <remarks><c>IsOpen</c> pops the popover up.</remarks>
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
			{
				picker.TimeChanged -= OnTimeChanged;
				picker.RemoveNotification("active", OnActiveChanged);
			}

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
