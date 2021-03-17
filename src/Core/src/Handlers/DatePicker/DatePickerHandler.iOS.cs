using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : AbstractViewHandler<IDatePicker, MauiDatePicker>
	{
		static UIDatePicker? Picker;
		static UIColor? DefaultTextColor;

		protected override MauiDatePicker CreateNativeView()
		{
			MauiDatePicker nativeDatePicker = new MauiDatePicker();

			Picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (NativeVersion.IsAtLeast(14))
			{
				Picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
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

			nativeDatePicker.InputView = Picker;
			nativeDatePicker.InputAccessoryView = toolbar;

			nativeDatePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			nativeDatePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			nativeDatePicker.InputAssistantItem.LeadingBarButtonGroups = null;
			nativeDatePicker.InputAssistantItem.TrailingBarButtonGroups = null;

			nativeDatePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			return nativeDatePicker;
		}

		protected override void ConnectHandler(MauiDatePicker nativeView)
		{
			if (Picker != null)
				Picker.ValueChanged += OnValueChanged;

			base.ConnectHandler(nativeView);
		}

		protected override void DisconnectHandler(MauiDatePicker nativeView)
		{
			if (Picker != null)
				Picker.ValueChanged -= OnValueChanged;

			base.DisconnectHandler(nativeView);
		}

		protected override void SetupDefaults(MauiDatePicker nativeView)
		{
			DefaultTextColor = nativeView.TextColor;

			base.SetupDefaults(nativeView);
		}

		public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateFormat(datePicker);
		}

		public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateDate(datePicker);
		}

		public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateMinimumDate(datePicker, Picker);
		}

		public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
		{
			handler.TypedNativeView?.UpdateMaximumDate(datePicker, Picker);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			SetVirtualViewDate();
		}

		void SetVirtualViewDate()
		{
			if (VirtualView == null || Picker == null)
				return;

			VirtualView.Date = Picker.Date.ToDateTime().Date;
		}
	}
}