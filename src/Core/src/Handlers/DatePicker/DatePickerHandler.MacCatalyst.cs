using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, UIDatePicker>
	{
		protected override UIDatePicker CreatePlatformView()
		{	
			return new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };;
		}

		protected override void ConnectHandler(UIDatePicker platformView)
		{
			platformView.EditingDidBegin += OnStarted;
			platformView.EditingDidEnd += OnEnded;
			platformView.ValueChanged += OnValueChanged;

			var date = VirtualView?.Date;
			if (date is DateTime dt)
			{
				platformView.Date = dt.ToNSDate();
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIDatePicker platformView)
		{
			platformView.EditingDidBegin -= OnStarted;
			platformView.EditingDidEnd -= OnEnded;
			platformView.ValueChanged -= OnValueChanged;

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

		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMinimumDate(datePicker);
		}

		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMaximumDate(datePicker);
		}

		public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
		}

		public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{

		}

		public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{
	
		}
    
		public static void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
		{
		
		}

		void OnValueChanged(object? sender, EventArgs? e)
		{
			SetVirtualViewDate();

			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = false;
		}

		void SetVirtualViewDate()
		{
			if (VirtualView == null)
				return;

			VirtualView.Date = PlatformView.Date.ToDateTime().Date;
		}
	}
}