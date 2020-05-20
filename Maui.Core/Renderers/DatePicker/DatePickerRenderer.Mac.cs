using AppKit;
using Foundation;

namespace System.Maui.Platform
{
	public partial class DatePickerRenderer : AbstractViewRenderer<IDatePicker, NSDatePicker>
	{
		//NSColor _defaultTextColor;
		//NSColor _defaultBackgroundColor;

		protected override NSDatePicker CreateView()
		{
			var nativeView = new MauiNSDatePicker
			{
				DatePickerMode = NSDatePickerMode.Single,
				TimeZone = new NSTimeZone("UTC"),
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
				DatePickerElements = NSDatePickerElementFlags.YearMonthDateDay
			};

			nativeView.ValidateProposedDateValue += HandleValueChanged;

			return nativeView;
		}

		protected override void DisposeView(NSDatePicker nativeView)
		{
			nativeView.ValidateProposedDateValue -= HandleValueChanged;
			base.DisposeView(nativeView);
		}

		public static void MapPropertyMaximumDate(IViewRenderer renderer, IDatePicker datePicker) { }
		public static void MapPropertyMinimumDate(IViewRenderer renderer, IDatePicker datePicker) { }
		public static void MapPropertySelectedDate(IViewRenderer renderer, IDatePicker datePicker) {
			(renderer as DatePickerRenderer)?.UpdateSelectedDate();
		}

		public virtual void UpdateSelectedDate()
		{
			var dt = VirtualView.SelectedDate.Date;
			if (TypedNativeView.DateValue.ToDateTime().Date != dt)
				TypedNativeView.DateValue = dt.ToNSDate();
		}

		void HandleValueChanged(object sender, NSDatePickerValidatorEventArgs e)
		{
			VirtualView.SelectedDate = e.ProposedDateValue.ToDateTime().Date;
		}
	}

	internal class MauiNSDatePicker : NSDatePicker
	{
		public event EventHandler<BoolEventArgs> FocusChanged;

		public override bool ResignFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(false));
			return base.ResignFirstResponder();
		}
		public override bool BecomeFirstResponder()
		{
			FocusChanged?.Invoke(this, new BoolEventArgs(true));
			return base.BecomeFirstResponder();
		}
	}

	internal class BoolEventArgs : EventArgs
	{
		public BoolEventArgs(bool value)
		{
			Value = value;
		}
		public bool Value
		{
			get;
			private set;
		}
	}
}
