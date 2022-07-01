using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
#if IOS && !MACCATALYST
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		protected override MauiDatePicker CreatePlatformView()
		{
			MauiDatePicker platformDatePicker = new MauiDatePicker();
			return platformDatePicker;
		}

#if !MACCATALYST
		static void OnDoneClicked(object sender)
		{
			if (sender is DatePickerHandler handler)
			{
				handler.SetVirtualViewDate();
				handler.PlatformView.ResignFirstResponder();
			}
		}
#endif

		internal UIDatePicker? DatePickerDialog { get { return PlatformView?.InputView as UIDatePicker; } }

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			PlatformView.SetDoneClicked(OnDoneClicked, this);
			PlatformView.SetDataContext(this);
			PlatformView.SetPickerDialogActions(OnStarted, null, null);

			if (DatePickerDialog is UIDatePicker picker)
			{
				//picker.EditingDidBegin += OnStarted;
				//picker.EditingDidEnd += OnEnded;
				//picker.ValueChanged += OnValueChanged;

				var date = VirtualView?.Date;
				if (date is DateTime dt)
				{
					picker.Date = dt.ToNSDate();
				}
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(MauiDatePicker platformView)
		{
			PlatformView.SetDoneClicked(null, null);
			PlatformView.SetPickerDialogActions(null, null, null);

			//if (DatePickerDialog is UIDatePicker picker)
			//{
			//	picker.EditingDidBegin -= OnStarted;
			//	picker.EditingDidEnd -= OnEnded;
			//	picker.ValueChanged -= OnValueChanged;
			//}

			base.DisconnectHandler(platformView);
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateFormat(datePicker, picker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?.DatePickerDialog;
			handler.PlatformView?.UpdateDate(datePicker, picker);
		}

		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler.DatePickerDialog);
		}

		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler.DatePickerDialog);
		}

		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
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

		public static void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(datePicker);
			handler.PlatformView?.UpdateTextAlignment(datePicker);
		}

		void OnValueChanged(object? sender, EventArgs? e)
		{
			if (UpdateImmediately)  // Platform Specific
				SetVirtualViewDate();

			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		static void OnStarted(object? sender)
		{
			if (sender is IDatePickerHandler datePickerHandler && datePickerHandler.VirtualView != null)
				datePickerHandler.VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = false;
		}

		void SetVirtualViewDate()
		{
			if (VirtualView == null || DatePickerDialog == null)
				return;

			VirtualView.Date = DatePickerDialog.Date.ToDateTime().Date;
		}
	}
#endif
}
