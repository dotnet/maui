using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using EcoreMainloop = ElmSharp.EcoreMainloop;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, TEntry>
	{
		const string DialogTitle = "Choose Date";
		Lazy<IDateTimeDialog>? _lazyDialog;

		protected override TEntry CreateNativeView()
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			var entry = new EditfieldEntry(NativeParent)
			{
				IsSingleLine = true,
				HorizontalTextAlignment = TTextAlignment.Center,
				InputPanelShowByOnDemand = true,
				IsEditable = false
			};
			entry.SetVerticalTextAlignment(0.5);
			return entry;
		}

		protected override void ConnectHandler(TEntry nativeView)
		{
			_ = NativeParent ?? throw new ArgumentNullException(nameof(NativeParent));

			nativeView.TextBlockFocused += OnTextBlockFocused;
			nativeView.EntryLayoutFocused += OnFocused;
			nativeView.EntryLayoutUnfocused += OnUnfocused;

			_lazyDialog = new Lazy<IDateTimeDialog>(() =>
			{
				var dialog = new DateTimePickerDialog(NativeParent)
				{
					Title = DialogTitle
				};
				dialog.DateTimeChanged += OnDateTimeChanged;
				dialog.PickerOpened += OnPickerOpened;
				dialog.PickerClosed += OnPickerClosed;
				return dialog;
			});

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(TEntry nativeView)
		{
			if (_lazyDialog != null && _lazyDialog.IsValueCreated)
			{
				_lazyDialog.Value.DateTimeChanged -= OnDateTimeChanged;
				_lazyDialog.Value.PickerOpened -= OnPickerOpened;
				_lazyDialog.Value.PickerClosed -= OnPickerClosed;
				_lazyDialog.Value.Unrealize();
				_lazyDialog = null;
			}

			nativeView.TextBlockFocused -= OnTextBlockFocused;
			nativeView.EntryLayoutFocused -= OnFocused;
			nativeView.EntryLayoutUnfocused -= OnUnfocused;

			base.DisconnectHandler(nativeView);
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateDate(datePicker);
		}

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateTextColor(datePicker);
		}

		[MissingMapper]
		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker) { }

		protected virtual void OnDateTimeChanged(object? sender, DateChangedEventArgs dcea)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Date = dcea.NewDate.Date;
		}

		void OnTextBlockFocused(object? sender, EventArgs e)
		{
			if (VirtualView == null || NativeView == null || _lazyDialog == null)
				return;

			// For EFL Entry, the event will occur even if it is currently disabled.
			// If the problem is resolved, no conditional statement is required.
			if (VirtualView.IsEnabled)
			{
				var dialog = _lazyDialog.Value;
				dialog.DateTime = VirtualView.Date;
				dialog.MaximumDateTime = VirtualView.MaximumDate;
				dialog.MinimumDateTime = VirtualView.MinimumDate;
				// You need to call Show() after ui thread occupation because of EFL problem.
				// Otherwise, the content of the popup will not receive focus.
				EcoreMainloop.Post(() => dialog.Show());
			}
		}

		protected virtual void OnPickerOpened(object? sender, EventArgs args)
		{
		}

		protected virtual void OnPickerClosed(object? sender, EventArgs args)
		{
		}
	}
}