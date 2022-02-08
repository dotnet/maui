using System;
using Foundation;
using ObjCRuntime;
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

		protected override void ConnectHandler(MauiDatePicker nativeView)
		{
			if (_picker != null)
			{
				_picker.EditingDidBegin += OnStarted;
				_picker.EditingDidEnd += OnEnded;
				_picker.ValueChanged += OnValueChanged;
			}

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiDatePicker nativeView)
		{
			if (_picker != null)
			{
				_picker.EditingDidBegin -= OnStarted;
				_picker.EditingDidEnd -= OnEnded;
				_picker.ValueChanged -= OnValueChanged;
			}

			base.DisconnectHandler(nativeView);
		}

		void SetupDefaults(MauiDatePicker nativeView)
		{
			_defaultTextColor = nativeView.TextColor;
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMinimumDate(datePicker, handler._picker);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMaximumDate(datePicker, handler._picker);
		}

		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(datePicker, fontManager);
		}

		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateTextColor(datePicker, handler._defaultTextColor);
		}

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			// TODO: Update IsFocused property
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			// TODO: Update IsFocused property
		}

		void OnValueChanged(object? sender, EventArgs? e)
		{
			SetVirtualViewDate();
		}

		void SetVirtualViewDate()
		{
			if (VirtualView == null || _picker == null)
				return;

			VirtualView.Date = _picker.Date.ToDateTime().Date;
		}
	}
}