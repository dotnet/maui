using System;
using System.Collections.Generic;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.DateTimePickerGalleries
{
	public partial class TimePage : ContentPage
	{
		public TimePage()
		{
			InitializeComponent();
			BindingContext = this;
			var dep = DependencyService.Get<ILocalize>();
			if (dep != null)
			{
				timephoneculture.Text = $"Device Culture: {dep.GetCurrentCultureInfo()}";
			}
			else
			{
				var s = System.Globalization.CultureInfo.CurrentCulture.Name;
				timephoneculture.Text = "Device Culture: " + s;
			}
		}

		void timeformat_Completed(System.Object sender, System.EventArgs e)
		{
			var text = ((Entry)sender).Text;
			timepicker.Format = text;
		}

		void timeSetting_Completed(System.Object sender, System.EventArgs e)
		{
			var time = ((Entry)sender).Text;
			string[] divided = time.Split(',');
			int[] sep_times = Array.ConvertAll(divided, int.Parse);
			timepicker.Time = new TimeSpan(sep_times[0], sep_times[1], 00);
		}
	}
}
