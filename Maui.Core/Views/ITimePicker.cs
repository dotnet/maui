namespace System.Maui
{
	public interface ITimePicker : IText
	{
		TimeSpan SelectedTime { get; set; }
		string ClockIdentifier { get; }
	}

	public static class ClockIdentifiers
	{
		public const string TwelveHour = "12HourClock";
		public const string TwentyFourHour = "24HourClock";
	}
}
