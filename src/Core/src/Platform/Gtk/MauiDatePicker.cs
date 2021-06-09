using System;
using System.Runtime.Serialization;

namespace Microsoft.Maui
{
	public class MauiDatePicker : Gtk.Label
	{

		public MauiDatePicker() : base()
		{
			Format = string.Empty;
		}

#pragma warning disable 169
		// to simulate
		// Tapping either of the DatePicker displays invokes the platform date picker
		Gtk.Calendar? _calendar;
#pragma warning restore 169
		
		DateTime _time;

		public DateTime Date
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