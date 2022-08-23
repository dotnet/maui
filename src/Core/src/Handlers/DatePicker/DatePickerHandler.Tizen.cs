using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EcoreMainloop = ElmSharp.EcoreMainloop;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, TEntry>
	{
		const string DialogTitle = "Choose Date";
		Lazy<IDateTimeDialog>? _lazyDialog;

		protected override TEntry CreatePlatformView()
		{
			var entry = new EditfieldEntry(PlatformParent)
			{
				IsSingleLine = true,
				HorizontalTextAlignment = TTextAlignment.Center,
				InputPanelShowByOnDemand = true,
				IsEditable = false
			};
			entry.SetVerticalTextAlignment(0.5);
			return entry;
		}

		protected override void ConnectHandler(TEntry platformView)
		{
			platformView.TextBlockFocused += OnTextBlockFocused;
			platformView.EntryLayoutFocused += OnFocused;
			platformView.EntryLayoutUnfocused += OnUnfocused;

			_lazyDialog = new Lazy<IDateTimeDialog>(() =>
			{
				var dialog = new DateTimePickerDialog(PlatformParent)
				{
					Title = DialogTitle
				};
				dialog.DateTimeChanged += OnDateTimeChanged;
				dialog.PickerOpened += OnPickerOpened;
				dialog.PickerClosed += OnPickerClosed;
				return dialog;
			});

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(TEntry platformView)
		{
			if (_lazyDialog != null && _lazyDialog.IsValueCreated)
			{
				_lazyDialog.Value.DateTimeChanged -= OnDateTimeChanged;
				_lazyDialog.Value.PickerOpened -= OnPickerOpened;
				_lazyDialog.Value.PickerClosed -= OnPickerClosed;
				_lazyDialog.Value.Unrealize();
				_lazyDialog = null;
			}

			platformView.TextBlockFocused -= OnTextBlockFocused;
			platformView.EntryLayoutFocused -= OnFocused;
			platformView.EntryLayoutUnfocused -= OnUnfocused;

			base.DisconnectHandler(platformView);
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateTextColor(datePicker);
		}

		[MissingMapper]
		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker) { }

		[MissingMapper]
		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker) { }

		protected virtual void OnDateTimeChanged(object? sender, DateChangedEventArgs dcea)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Date = dcea.NewDate.Date;
		}

		void OnTextBlockFocused(object? sender, EventArgs e)
		{
			if (VirtualView == null || PlatformView == null || _lazyDialog == null)
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