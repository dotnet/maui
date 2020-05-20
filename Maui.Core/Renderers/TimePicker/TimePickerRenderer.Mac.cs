using AppKit;
using Foundation;

namespace System.Maui.Platform
{
	public partial class TimePickerRenderer : AbstractViewRenderer<ITimePicker, NSDatePicker>
	{
		protected override NSDatePicker CreateView()
		{
			var nativeView = new MauiNSDatePicker
			{
				DatePickerMode = NSDatePickerMode.Single,
				TimeZone = new NSTimeZone("UTC"),
				DatePickerStyle = NSDatePickerStyle.TextFieldAndStepper,
				DatePickerElements = NSDatePickerElementFlags.HourMinuteSecond
			};

			nativeView.ValidateProposedDateValue += HandleValueChanged;

			return nativeView;
		}

		protected override void DisposeView(NSDatePicker nativeView)
		{
			nativeView.ValidateProposedDateValue -= HandleValueChanged;
			base.DisposeView(nativeView);
		}

		public static void MapPropertySelectedTime(IViewRenderer renderer, ITimePicker timePicker)
		{
			(renderer as TimePickerRenderer)?.UpdateSelectedTime();
		}

		public virtual void UpdateSelectedTime()
		{
			var time = new DateTime(2001, 1, 1).Add(VirtualView.SelectedTime);
			var newDate = time.ToNSDate();
			if (!Equals(TypedNativeView.DateValue, newDate))
				TypedNativeView.DateValue = newDate;
		}

		void HandleValueChanged(object sender, NSDatePickerValidatorEventArgs e)
		{
			VirtualView.SelectedTime = e.ProposedDateValue.ToDateTime().Date - new DateTime(2001, 1, 1);
		}

	}
}
