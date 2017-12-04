using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Extends the ElmSharp.DateTimeSelector class with functionality useful to renderer.
	/// </summary>
	public class DatePicker : DateTimeSelector
	{
		const string DateLayoutStyle = "date_layout";
		const string DefaultEFLFormat = "%d/%b/%Y";
		static readonly DateTime s_defaultMaximumDate = new DateTime(2037, 12, 31);
		static readonly DateTime s_defaultMinimumDate = new DateTime(1970, 1, 1);
		DateTime _date;
		DateTime _maxDate;
		DateTime _minDate;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatePicker"/> class.
		/// </summary>
		/// <param name="parent">The parent EvasObject.</param>
		public DatePicker(EvasObject parent) : base(parent)
		{
			SetFieldVisible(DateTimeFieldType.Hour, false);
			Style = DateLayoutStyle;
			ApplyDate(Date);
			ApplyMinDate(s_defaultMinimumDate);
			ApplyMaxDate(s_defaultMaximumDate);
			//TODO use date format currently set on the platform
			Format = DefaultEFLFormat;

			DateTimeChanged += (sender, e) =>
			{
				Date = e.NewDate;
			};
		}

		/// <summary>
		/// Gets or sets the displayed date.
		/// </summary>
		public DateTime Date
		{
			get
			{
				return _date;
			}
			set
			{
				if (_date != value)
				{
					ApplyDate(value);
				}
			}
		}

		/// <summary>
		/// Gets of sets the highest date selectable for this <see cref="DatePicker"/>.
		/// </summary>
		/// <remarks>
		/// Default value is 31st Dec, 2037.
		/// </remarks>
		public DateTime MaximumDate
		{
			get
			{
				return _maxDate;
			}
			set
			{
				if (_maxDate != value)
				{
					ApplyMaxDate(value);
				}
			}
		}

		/// <summary>
		/// Gets of sets the lowest date selectable for this <see cref="DatePicker"/>.
		/// </summary>
		/// <remarks>
		/// Default value is 1st Jan, 1970.
		/// </remarks>
		public DateTime MinimumDate
		{
			get
			{
				return _minDate;
			}
			set
			{
				if (_minDate != value)
				{
					ApplyMinDate(value);
				}
			}
		}

		/// <summary>
		/// Sets the <c>DateTime</c> property according to the given <paramref name="date"/>.
		/// </summary>
		/// <param name="date">The date value to be applied to the date picker.</param>
		void ApplyDate(DateTime date)
		{
			_date = date;
			DateTime = date;
		}

		/// <summary>
		/// Sets the <c>MaximumDateTime</c> property according to the given <paramref name="maxDate"/>.
		/// </summary>
		/// <param name="maxDate">The maximum date value to be applied to the date picker.</param>
		void ApplyMaxDate(DateTime maxDate)
		{
			_maxDate = maxDate;
			MaximumDateTime = maxDate;
		}

		/// <summary>
		/// Sets the <c>MinimumDateTime</c> property according to the given <paramref name="minDate"/>.
		/// </summary>
		/// <param name="minDate">The minimum date value to be applied to the date picker.</param>
		void ApplyMinDate(DateTime minDate)
		{
			_minDate = minDate;
			MinimumDateTime = minDate;
		}
	}
}

