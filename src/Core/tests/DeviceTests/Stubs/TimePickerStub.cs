using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class TimePickerStub : StubBase, ITimePicker
	{
		TimeSpan _time;

		public string Format { get; set; }

		public TimeSpan Time
		{
			get => _time;
			set => SetProperty(ref _time, value);
		}

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }
	}
}