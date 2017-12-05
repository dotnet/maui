using System;
using System.Linq;
using System.Text.RegularExpressions;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ElmSharp.DateTimeSelector class with functionality useful to renderer.
	/// </summary>
	public class TimePicker : DateTimeSelector
	{
		// Remark : This modification is temporary patch because of EFL crash
		const string DefaultEFLFormat = "%d/%b/%Y %I:%M %p";
		//TODO need to add internationalization support
		const string FormatExceptionMessage = "Input string was not in a correct format.";
		const string RegexValidTimePattern = "^([h]{1,2}|[H]{1,2})[.:-]([m]{1,2})(([.:-][s]{1,2})?)(([.:-][fF]{1,7})?)(([K])?)(([z]{1,3})?)(([ ][t]{1,2})?)$";
		const string TimeLayoutStyle = "time_layout";
		string _dateTimeFormat;
		TimeSpan _time;

		/// <summary>
		/// Initializes a new instance of the <see cref="TimePicker"/> class.
		/// </summary>
		/// <param name="parent">The parent EvasObject.</param>
		public TimePicker(EvasObject parent) : base(parent)
		{
			Style = TimeLayoutStyle;
			ApplyTime(Time);
			ApplyFormat(DateTimeFormat);

			DateTimeChanged += (sender, e) =>
			{
				Time = e.NewDate.TimeOfDay;
			};
		}

		/// <summary>
		/// Gets or sets the displayed date time format.
		/// </summary>
		public string DateTimeFormat
		{
			get
			{
				return _dateTimeFormat;
			}
			set
			{
				if (_dateTimeFormat != value)
				{
					ApplyFormat(value);
				}
			}
		}

		/// <summary>
		/// Gets or sets the displayed time.
		/// </summary>
		public TimeSpan Time
		{
			get
			{
				return _time;
			}
			set
			{
				if (_time != value)
				{
					ApplyTime(value);
				}
			}
		}

		/// <summary>
		/// Sets the <c>Format</c> property according to the given <paramref name="format"/>.
		/// </summary>
		/// <param name="format">The format value to be applied to the time picker.</param>
		void ApplyFormat(string format)
		{
			_dateTimeFormat = format;
			Format = ConvertToEFLFormat(_dateTimeFormat);
		}

		/// <summary>
		/// Sets the <c>DateTime</c> property according to the given <paramref name="time"/>.
		/// </summary>
		/// <param name="time">The time value to be applied to the time picker.</param>
		void ApplyTime(TimeSpan time)
		{
			_time = time;
			DateTime = ConvertToDateTime(time);
		}

		/// <summary>
		/// Converts parameter <paramref name="timeSpan"/> to <see cref="DateTime"/>.
		/// </summary>
		/// <param name="timeSpan">The time value to be converted to <see cref="DateTime"/>.</param>
		/// <returns>An object representing the date 1st Jan, 1970 (minimum date of ElmSharp.DateTimeSelector) with added <paramref name="timeSpan"/>.</returns>
		DateTime ConvertToDateTime(TimeSpan timeSpan)
		{
			return new DateTime(1970, 1, 1) + timeSpan;
		}

		/// <summary>
		/// Converts standard or custom <see cref="DateTime"/> format to EFL format.
		/// </summary>
		/// <param name="dateTimeFormat">The <see cref="DateTime"/> format to be converted to EFL format.</param>
		/// <exception cref="FormatException"><param name="dateTimeFormat"> does not contain a valid string representation of a date and time.</exception>
		/// <returns>An object representing the EFL time format string.
		/// Example:
		/// "t" or "T" returns default EFL format "%I:%M %p"
		/// "HH:mm tt" returns "%H:%M %p"
		/// "h:mm" returns "%l:%M"
		/// </returns>
		string ConvertToEFLFormat(string dateTimeFormat)
		{
			if (string.IsNullOrWhiteSpace(dateTimeFormat))
			{
				return DefaultEFLFormat;
			}

			if (dateTimeFormat.Length == 1)
			{
				//Standard Time Format (DateTime)
				if (dateTimeFormat[0] == 't' || dateTimeFormat[0] == 'T')
				{
					return DefaultEFLFormat;
				}
				else
				{
					throw new FormatException(FormatExceptionMessage);
				}
			}
			else
			{
				//Custom Time Format (DateTime)
				Regex regex = new Regex(RegexValidTimePattern);
				if (!regex.IsMatch(dateTimeFormat))
				{
					throw new FormatException(FormatExceptionMessage);
				}

				string format = string.Empty;
				int count_h = dateTimeFormat.Count(m => m == 'h'); //12h
				int count_H = dateTimeFormat.Count(m => m == 'H'); //24h

				if (count_h == 1)
				{
					format += "%l";
				}
				else if (count_h == 2)
				{
					format += "%I";
				}
				else if (count_H == 1)
				{
					format += "%k";
				}
				else if (count_H == 2)
				{
					format += "%H";
				}

				format += ":%M";
				int count_t = dateTimeFormat.Count(m => m == 't');

				if ((count_H > 0 && count_t > 0) ||
					(count_h > 0 && count_t == 0))
				{
					throw new FormatException(FormatExceptionMessage);
				}

				if (count_t == 1)
				{
					format += " %P";
				}
				else if (count_t == 2)
				{
					format += " %p";
				}

				return format;
			}
		}
	}
}
