namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33583, "DatePicker does not update MinimumDate / MaximumDate in the Popup after first opening", PlatformAffected.Android)]
public class Issue33583 : TestContentPage
{
    DatePicker _datePicker;

    protected override void Init()
    {
        _datePicker = new DatePicker
        {
            AutomationId = "TestDatePicker",
            HorizontalOptions = LayoutOptions.Center,
            MinimumDate = new DateTime(2025, 1, 4),
            MaximumDate = new DateTime(2025, 1, 28),
            Date = new DateTime(2025, 1, 15)
        };

        var changeMaxDateButton = new Button
        {
            Text = "Change MaximumDate to 2027",
            AutomationId = "ChangeMaxDateButton"
        };
        changeMaxDateButton.Clicked += (s, e) =>
        {
            _datePicker.MaximumDate = new DateTime(2027, 12, 31);
        };

        Content = new VerticalStackLayout
        {
            Spacing = 10,
            Children =
            {
                _datePicker,
                changeMaxDateButton
            }
        };
    }
}
