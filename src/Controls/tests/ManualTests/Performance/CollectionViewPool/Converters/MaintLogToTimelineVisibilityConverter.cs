using System;
using System.Collections.Generic;
using System.Globalization;
using PoolMath.Data;


namespace PoolMathApp.Xaml
{
	public class MaintLogToTimelineVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var maintenanceLog = value as MaintenanceLog;
			if (maintenanceLog != null)
			{
				if (parameter.ToString().Equals("action", StringComparison.OrdinalIgnoreCase))
					return maintenanceLog.Vacuumed || maintenanceLog.CleanedFilter || maintenanceLog.Brushed || maintenanceLog.Backwashed;
				else
					return maintenanceLog.Pressure.HasValue || maintenanceLog.SWGCellPercent.HasValue || maintenanceLog.WaterTemp.HasValue || maintenanceLog.FlowRate.HasValue || maintenanceLog.PumpRunTime.HasValue;
			}

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
