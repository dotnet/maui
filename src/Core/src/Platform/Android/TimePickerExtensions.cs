namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.SetTime(timePicker);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.SetTime(timePicker);
		}

		public static void UpdateCharacterSpacing(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.LetterSpacing = timePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, IFontManager fontManager) =>
			mauiTimePicker.UpdateFont(timePicker.Font, fontManager);

		internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format);
		}
	}
}