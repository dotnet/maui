using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;
using EcoreMainloop = ElmSharp.EcoreMainloop;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : EViewHandler<ITimePicker, TEntry>
	{
		const string DialogTitle = "Choose Time";
		Lazy<IDateTimeDialog>? _lazyDialog;

		protected override TEntry CreateNativeView()
		{
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
			nativeView.TextBlockFocused += OnTextBlockFocused;
			nativeView.EntryLayoutFocused += OnFocused;
			nativeView.EntryLayoutUnfocused += OnUnfocused;

			_lazyDialog = new Lazy<IDateTimeDialog>(() =>
			{
				var dialog = new DateTimePickerDialog(NativeParent)
				{
					Mode = DateTimePickerMode.Time,
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

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateFormat(timePicker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateTime(timePicker);
		}

		public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.NativeView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.NativeView?.UpdateTextColor(timePicker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker) { }

		protected virtual void OnDateTimeChanged(object? sender, DateChangedEventArgs dcea)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.Time = dcea.NewDate.TimeOfDay;
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
				dialog.DateTime -= dialog.DateTime.TimeOfDay;
				dialog.DateTime += VirtualView.Time;
				
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