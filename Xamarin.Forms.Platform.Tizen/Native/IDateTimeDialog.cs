using System;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public interface IDateTimeDialog
	{
		string Title { get; set; }
		DateTimePickerMode Mode { get; set; }
		DateTime MaximumDateTime { get; set; }
		DateTime MinimumDateTime { get; set; }
		DateTime DateTime { get; set; }

		event EventHandler<DateChangedEventArgs> DateTimeChanged;
		event EventHandler PickerOpened;
		event EventHandler PickerClosed;

		void Show();
		void Hide();
		void Unrealize();
	}
}
