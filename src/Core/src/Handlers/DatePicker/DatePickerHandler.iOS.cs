using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		UIColor? _defaultTextColor;
		UIDatePicker? _picker;

		protected override MauiDatePicker CreatePlatformView()
		{
			MauiDatePicker platformDatePicker = new MauiDatePicker();

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (PlatformVersion.IsAtLeast(14))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			var width = UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				SetVirtualViewDate();
				platformDatePicker.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			platformDatePicker.InputView = _picker;
			platformDatePicker.InputAccessoryView = toolbar;

			platformDatePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			platformDatePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			platformDatePicker.InputAssistantItem.LeadingBarButtonGroups = null;
			platformDatePicker.InputAssistantItem.TrailingBarButtonGroups = null;

			platformDatePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			return platformDatePicker;
		}

		internal UIDatePicker? DatePickerDialog { get { return _picker; } }

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

			SetupDefaults(platformView);

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

		void SetupDefaults(MauiDatePicker platformView)
		{
			_defaultTextColor = platformView.TextColor;
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
			if (handler is DatePickerHandler platformHandler)
				handler.PlatformView?.UpdateTextColor(datePicker, platformHandler._defaultTextColor);
		}
    
		public static void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(datePicker);
			handler.PlatformView?.UpdateTextAlignment(datePicker);
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
			if (VirtualView == null || _picker == null)
				return;

			VirtualView.Date = _picker.Date.ToDateTime().Date;
		}
	}
}