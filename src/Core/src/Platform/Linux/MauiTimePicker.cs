using System;

namespace Microsoft.Maui
{
	public class MauiTimePicker : Gtk.Label
	{

		public MauiTimePicker() : base()
		{
			Format = string.Empty;
		}

		TimeSpan _time;

		public TimeSpan Time
		{
			get => _time;
			set
			{
				_time = value;
				Text = _time.ToString();
			}
		}

		public string Format { get; set; }

	}
}