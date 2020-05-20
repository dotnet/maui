namespace System.Maui
{
	public interface IDatePicker : IText
	{ 
		DateTime SelectedDate { get; set; }
		DateTime MinimumDate { get; }
		DateTime MaximumDate { get; }

		string IText.Text => SelectedDate.ToShortDateString();
	}
}
