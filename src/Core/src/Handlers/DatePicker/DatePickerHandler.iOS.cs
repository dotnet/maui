using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
#if !MACCATALYST
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		protected override MauiDatePicker CreatePlatformView()
		{
			MauiDatePicker platformDatePicker = new MauiDatePicker();
			return platformDatePicker;
		}

		internal UIDatePicker? DatePickerDialog { get { return PlatformView?.InputView as UIDatePicker; } }

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			platformView.MauiDatePickerDelegate = new DatePickerDelegate(this);

			if (DatePickerDialog is UIDatePicker picker)
			{
				var date = VirtualView?.Date;
				if (date is not null && date is DateTime dt)
				{
					picker.Date = dt.ToNSDate();
				}
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			platformView.MauiDatePickerDelegate = null;

			base.DisconnectHandler(platformView);
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateFormat(datePicker, picker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateDate(datePicker, picker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler.DatePickerDialog);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler.DatePickerDialog);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateTextColor(datePicker);
		}

		public static partial void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(datePicker);
			handler.PlatformView?.UpdateTextAlignment(datePicker);
		}

		internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateIsOpen(datePicker);
		}

		static void OnValueChanged(object? sender)
		{
			if (sender is DatePickerHandler datePickerHandler)
			{
				if (datePickerHandler.UpdateImmediately)  // Platform Specific
					datePickerHandler.SetVirtualViewDate();

				if (datePickerHandler.VirtualView != null)
					datePickerHandler.VirtualView.IsFocused = true;
			}
		}

		static void OnStarted(object? sender)
		{
			if (sender is IDatePickerHandler datePickerHandler && datePickerHandler.VirtualView != null)
				datePickerHandler.VirtualView.IsFocused = datePickerHandler.VirtualView.IsOpen = true;
		}

		static void OnEnded(object? sender)
		{
			if (sender is IDatePickerHandler datePickerHandler && datePickerHandler.VirtualView != null)
				datePickerHandler.VirtualView.IsFocused = datePickerHandler.VirtualView.IsOpen = false;
		}

		static void OnDoneClicked(object? sender)
		{
			if (sender is DatePickerHandler handler)
			{
				handler.SetVirtualViewDate();
				handler.PlatformView.ResignFirstResponder();
			}
		}

		void SetVirtualViewDate()
		{
			if (VirtualView is null || DatePickerDialog is null)
			{
				return;
			}

			VirtualView.Date = DatePickerDialog.Date.ToDateTime();
		}

		class DatePickerDelegate : MauiDatePickerDelegate
		{
			readonly WeakReference<IDatePickerHandler> _handler;

			public DatePickerDelegate(IDatePickerHandler handler) =>
				_handler = new WeakReference<IDatePickerHandler>(handler);

			IDatePickerHandler? Handler
			{
				get
				{
					if (_handler?.TryGetTarget(out IDatePickerHandler? target) == true)
						return target;

					return null;
				}
			}

			public override void DatePickerEditingDidBegin()
			{
				DatePickerHandler.OnStarted(Handler);
			}

			public override void DatePickerEditingDidEnd()
			{
				DatePickerHandler.OnEnded(Handler);
			}

			public override void DatePickerValueChanged()
			{
				DatePickerHandler.OnValueChanged(Handler);
			}

			public override void DoneClicked()
			{
				DatePickerHandler.OnDoneClicked(Handler);
			}
		}
	}
#endif
}
