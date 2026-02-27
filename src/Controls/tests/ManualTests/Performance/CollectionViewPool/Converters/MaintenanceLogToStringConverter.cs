using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PoolMath.Data;


namespace PoolMathApp.Xaml
{
	public class MaintenanceLogToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var maintenanceLog = value as MaintenanceLog;
			if (maintenanceLog != null)
			{
				var jobs = new List<string>();
				if (maintenanceLog.Vacuumed)
					jobs.Add("Vacuumed");
				if (maintenanceLog.CleanedFilter)
					jobs.Add("Cleaned Filter");
				if (maintenanceLog.Backwashed)
					jobs.Add("Backwashed");
				if (maintenanceLog.Brushed)
					jobs.Add("Brushed");

				if (jobs.Any())
					return string.Join(", ", jobs);
			}
			return " ";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
