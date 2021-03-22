namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker view)
		{
			mauiTimePicker.SetTime(view);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker view)
		{
			mauiTimePicker.SetTime(view);
		}

		internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format);
		}
	}
}