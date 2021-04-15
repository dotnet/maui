using System;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, MauiDatePicker>
	{
		UIDatePicker? _picker;

		protected override MauiDatePicker CreateNativeView()
		{
			MauiDatePicker nativeDatePicker = new MauiDatePicker();

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (NativeVersion.IsAtLeast(14))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			var width = UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				SetVirtualViewDate();
				nativeDatePicker.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			nativeDatePicker.InputView = _picker;
			nativeDatePicker.InputAccessoryView = toolbar;

			nativeDatePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			nativeDatePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			nativeDatePicker.InputAssistantItem.LeadingBarButtonGroups = null;
			nativeDatePicker.InputAssistantItem.TrailingBarButtonGroups = null;

			nativeDatePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			return nativeDatePicker;
		}

		internal UIDatePicker? DatePickerDialog { get { return _picker; } }

		protected override void ConnectHandler(MauiDatePicker nativeView)
		{
			if (_picker != null)
				_picker.ValueChanged += OnValueChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiDatePicker nativeView)
		{
			if (_picker != null)
				_picker.ValueChanged -= OnValueChanged;

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

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateMinimumDate(datePicker, handler._picker);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateMaximumDate(datePicker, handler._picker);
		}

		public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.NativeView?.UpdateCharacterSpacing(datePicker);
		}

		public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(datePicker, fontManager);
		}

		[MissingMapper]
		public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker) { }

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