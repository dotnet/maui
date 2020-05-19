using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public interface IDateTimeDialog
	{
		string Title { get; set; }
		DateTimePicker Picker { get; }
		event EventHandler<DateChangedEventArgs> DateTimeChanged;
		void Show();
		void Hide();
		void Unrealize();
	}
}
