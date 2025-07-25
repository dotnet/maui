#if NET6_0_OR_GREATER
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20438, "Add DateOnly and TimeOnly converters to DatePicker and TimePicker", PlatformAffected.All)]
public class Issue20438 : ContentPage
{
    public Issue20438()
    {
        Title = "Page";
        var viewModel = new Issue20438ViewModel();
        BindingContext = viewModel;

        var datePickerDateOnly = new DatePicker { AutomationId = "TestDatePickerWithDateOnly" };
        datePickerDateOnly.SetBinding(DatePicker.DateProperty, "DateWithDateOnly");
        var labelDateOnly = new Label { AutomationId = "LabelDateOnly" };

        var datePickerDateTime = new DatePicker { AutomationId = "TestDatePickerWithDateTime" };
        datePickerDateTime.SetBinding(DatePicker.DateProperty, "DateWithDateTime");
        var labelDateTime = new Label { AutomationId = "LabelDateTime" };

        var datePickerString = new DatePicker { AutomationId = "TestDatePickerWithString" };
        datePickerString.SetBinding(DatePicker.DateProperty, "DateWithString");
        var labelString = new Label { AutomationId = "LabelString" };

        var timePickerTimeOnly = new TimePicker { AutomationId = "TestTimePickerWithTimeOnly" };
        timePickerTimeOnly.SetBinding(TimePicker.TimeProperty, "TimeWithTimeOnly");
        var labelTimeOnly = new Label { AutomationId = "LabelTimeOnly" };

        var timePickerTimeSpan = new TimePicker { AutomationId = "TestTimePickerWithTimeSpan" };
        timePickerTimeSpan.SetBinding(TimePicker.TimeProperty, "TimeWithTimeSpan");
        var labelTimeSpan = new Label { AutomationId = "LabelTimeSpan" };

        var timePickerString = new TimePicker { AutomationId = "TestTimePickerWithString" };
        timePickerString.SetBinding(TimePicker.TimeProperty, "TimeWithString");
        var labelTimeString = new Label { AutomationId = "LabelTimeString" };

        var datePickerMinMax = new DatePicker { AutomationId = "TestDatePickerWithMinMax" };
        datePickerMinMax.SetBinding(DatePicker.DateProperty, "DateWithDateTime");
        datePickerMinMax.SetBinding(DatePicker.MinimumDateProperty, "MinDateOnly");
        datePickerMinMax.SetBinding(DatePicker.MaximumDateProperty, "MaxDateOnly");
        var labelMinMax = new Label { AutomationId = "LabelMinMax" };

        Content = new VerticalStackLayout
        {
            Children =
            {
                datePickerDateOnly, labelDateOnly,
                datePickerDateTime, labelDateTime,
                datePickerString, labelString,
                timePickerTimeOnly, labelTimeOnly,
                timePickerTimeSpan, labelTimeSpan,
                timePickerString, labelTimeString,
                datePickerMinMax, labelMinMax
            }
        };

        labelDateOnly.Text = $"DateOnly Value: {datePickerDateOnly.Date:yyyy-MM-dd}";
        labelDateTime.Text = $"DateTime Value: {datePickerDateTime.Date:yyyy-MM-dd}";
        labelString.Text = $"String Value: {datePickerString.Date:yyyy-MM-dd}";
        labelTimeOnly.Text = $"TimeOnly Value: {timePickerTimeOnly.Time:hh\\:mm\\:ss}";
        labelTimeSpan.Text = $"TimeSpan Value: {timePickerTimeSpan.Time:hh\\:mm\\:ss}";
        labelTimeString.Text = $"Time String Value: {timePickerString.Time:hh\\:mm\\:ss}";
        labelMinMax.Text = $"MinMax DatePicker: Min={datePickerMinMax.MinimumDate:yyyy-MM-dd}, Max={datePickerMinMax.MaximumDate:yyyy-MM-dd}";
    }
}

public class Issue20438ViewModel : INotifyPropertyChanged
{
    public DateOnly DateWithDateOnly { get; set; } = new(2025, 7, 28);
    public TimeOnly TimeWithTimeOnly { get; set; } = new(10, 30, 0);
    public TimeSpan TimeWithTimeSpan { get; set; } = new(10, 30, 0);
    public DateTime DateWithDateTime { get; set; } = new(2025, 7, 28, 10, 30, 0);
    public string DateWithString { get; set; } = "2025-07-28";
    public string TimeWithString { get; set; } = "10:30:00";
    public DateOnly MinDateOnly { get; set; } = new(2025, 7, 1);
    public DateOnly MaxDateOnly { get; set; } = new(2025, 8, 31);

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
#endif
