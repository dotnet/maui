namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27803, "DatePicker default format on iOS", PlatformAffected.iOS)]
public partial class Issue27803 : ContentPage
{
	private DateTime workDate = new DateTime(2025, 2, 25);
	public DateTime WorkDate
	{
		get => workDate;
		set
		{
			workDate = value;
			OnPropertyChanged();
		}
	}

	private Color datePickerTextColor = Color.FromArgb("#0000FF");
	public Color DatePickerTextColor
	{
		get => datePickerTextColor;
		set
		{
			datePickerTextColor = value;
			OnPropertyChanged();
		}
	}
	public Issue27803()
	{
		InitializeComponent();
		BindingContext = this;

	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		DatePickerTextColor = Color.FromArgb("#FF0000");

	}
}