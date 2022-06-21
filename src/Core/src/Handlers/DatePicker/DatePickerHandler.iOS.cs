using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
#if IOS && !MACCATALYST
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		UIDatePicker? _picker;

		protected override MauiDatePicker CreatePlatformView()
		{
			MauiDatePicker platformDatePicker = new MauiDatePicker();

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			platformDatePicker.InputView = _picker;
			var accessoryView = new MauiDoneAccessoryView();
			accessoryView.DoneClicked += (_, _) =>
			{
				SetVirtualViewDate();
				PlatformView.ResignFirstResponder();
			};

			platformDatePicker.InputAccessoryView = accessoryView;

			//platformDatePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			////platformDatePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			//platformDatePicker.InputAssistantItem.LeadingBarButtonGroups = null;
			//platformDatePicker.InputAssistantItem.TrailingBarButtonGroups = null;

			//platformDatePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			return platformDatePicker;
		}

		internal UIDatePicker? DatePickerDialog { get { return _picker; } }

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiDatePicker platformView)
		{
			if (_picker is UIDatePicker picker)
			{
				picker.EditingDidBegin += OnStarted;
				picker.EditingDidEnd += OnEnded;
				picker.ValueChanged += OnValueChanged;

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
			if (_picker != null)
			{
				_picker.EditingDidBegin -= OnStarted;
				_picker.EditingDidEnd -= OnEnded;
				_picker.ValueChanged -= OnValueChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?._picker;
			handler.PlatformView?.UpdateFormat(datePicker, picker);
		}

		public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			var picker = (handler as DatePickerHandler)?._picker;
			handler.PlatformView?.UpdateDate(datePicker, picker);
		}

		public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMinimumDate(datePicker, platformHandler._picker);
		}

		public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateMaximumDate(datePicker, platformHandler._picker);
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
			if (VirtualView == null || _picker == null)
				return;

			VirtualView.Date = _picker.Date.ToDateTime().Date;
		}
	}
#endif
}
