using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DateTimePickerGalleries
{
	public partial class DatePage : ContentPage
	{
		public DatePage()
		{
			InitializeComponent();
			BindingContext = this;
			var dep = DependencyService.Get<ILocalize>();
			if (dep != null)
			{
				datesphoneculture.Text = $"Device Culture: {dep.GetCurrentCultureInfo()}";
			}
			else
			{
				var s = System.Globalization.CultureInfo.CurrentCulture.Name;
				datesphoneculture.Text = "Device Culture: " + s;
			}
		}

		void dateformat_Completed(System.Object sender, System.EventArgs e)
		{
			var text = ((Entry)sender).Text;
			datepicker.Format = text;
		}

		void dateSetting_Completed(System.Object sender, System.EventArgs e)
		{
			var date = ((Entry)sender).Text;
			string[] divided = date.Split(',');
			int[] sep_dates = Array.ConvertAll(divided, int.Parse);
			datepicker.Date = new DateTime(sep_dates[0], sep_dates[1], sep_dates[2]);
		}
	}
}
