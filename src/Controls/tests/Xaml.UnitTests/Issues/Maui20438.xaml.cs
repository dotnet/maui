#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20438 : ContentPage
{
    public Maui20438()
    {
        InitializeComponent();
    }

    [Collection("Issue")]
    public class Tests
    {
        [Theory]
        [XamlInflatorData]
        internal void DateOnlyBinding(XamlInflator inflator)
        {
            var page = new Maui20438(inflator);
            Assert.Equal(new DateTime(2025, 3, 15), page.datePicker.Date);
        }

        [Theory]
        [XamlInflatorData]
        internal void TimeOnlyBinding(XamlInflator inflator)
        {
            var page = new Maui20438(inflator);
            Assert.Equal(new TimeSpan(14, 30, 0), page.timePicker.Time);
        }

        [Theory]
        [XamlInflatorData]
        internal void DateOnlyToNonNullableDateTime(XamlInflator inflator)
        {
            var page = new Maui20438(inflator);
            Assert.Equal(new DateTime(2025, 3, 15), page.customDatePicker.NonNullableDateTime);
        }

        [Theory]
        [XamlInflatorData]
        internal void TimeOnlyToNonNullableTimeSpan(XamlInflator inflator)
        {
            var page = new Maui20438(inflator);
            Assert.Equal(new TimeSpan(14, 30, 0), page.customTimePicker.NonNullableTimeSpan);
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
