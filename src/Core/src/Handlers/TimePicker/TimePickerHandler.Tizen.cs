using System;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.ElmSharp;
using EcoreMainloop = ElmSharp.EcoreMainloop;
using TEntry = Tizen.UIExtensions.ElmSharp.Entry;
using TTextAlignment = Tizen.UIExtensions.Common.TextAlignment;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, TEntry>
	{
		const string DialogTitle = "Choose Time";
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
					Mode = DateTimePickerMode.Time,
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

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();
			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker);
		}

		[MissingMapper]
		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker) { }

		protected virtual void OnDateTimeChanged(object? sender, DateChangedEventArgs dcea)
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Time = dcea.NewDate.TimeOfDay;
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