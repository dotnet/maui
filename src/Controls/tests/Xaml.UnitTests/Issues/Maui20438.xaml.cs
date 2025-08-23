#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20438 : ContentPage
{
	public Maui20438()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void DateOnlyBinding([Values] XamlInflator inflator)
		{
			var page = new Maui20438(inflator);
			Assert.That(page.datePicker.Date, Is.EqualTo(new DateTime(2025, 3, 15)));
		}

		[Test]
		public void TimeOnlyBinding([Values] XamlInflator inflator)
		{
			var page = new Maui20438(inflator);
			Assert.That(page.timePicker.Time, Is.EqualTo(new TimeSpan(14, 30, 0)));
		}

		[Test]
		public void DateOnlyToNonNullableDateTime([Values] XamlInflator inflator)
		{
			var page = new Maui20438(inflator);
			Assert.That(page.customDatePicker.NonNullableDateTime, Is.EqualTo(new DateTime(2025, 3, 15)));
		}

		[Test]
		public void TimeOnlyToNonNullableTimeSpan([Values] XamlInflator inflator)
		{
			var page = new Maui20438(inflator);
			Assert.That(page.customTimePicker.NonNullableTimeSpan, Is.EqualTo(new TimeSpan(14, 30, 0)));
		}
	}
}

public class Issue20438CustomDatePicker : DatePicker
{
	public static readonly BindableProperty NonNullableDateTimeProperty =
		BindableProperty.Create(nameof(NonNullableDateTime), typeof(DateTime), typeof(Issue20438CustomDatePicker), default(DateTime));

	public DateTime NonNullableDateTime
	{
		get => (DateTime)GetValue(NonNullableDateTimeProperty);
		set => SetValue(NonNullableDateTimeProperty, value);
	}
}

public class Issue20438CustomTimePicker : TimePicker
{
	public static readonly BindableProperty NonNullableTimeSpanProperty =
		BindableProperty.Create(nameof(NonNullableTimeSpan), typeof(TimeSpan), typeof(Issue20438CustomTimePicker), default(TimeSpan));

	public TimeSpan NonNullableTimeSpan
	{
		get => (TimeSpan)GetValue(NonNullableTimeSpanProperty);
		set => SetValue(NonNullableTimeSpanProperty, value);
	}
}

public class Issue20438ViewModel
{
	public DateOnly SelectedDate { get; set; } = new DateOnly(2025, 3, 15);
	public TimeOnly SelectedTime { get; set; } = new TimeOnly(14, 30, 0);
}
#endif
