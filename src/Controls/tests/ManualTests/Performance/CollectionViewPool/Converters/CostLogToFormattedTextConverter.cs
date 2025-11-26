using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PoolMath;
using PoolMath.Data;
using PoolMathApp.Helpers;
using PoolMathApp.Models;

namespace PoolMathApp.Xaml
{
	public class CostLogToFormattedTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var result = new FormattedText();

			if (value is null)
				return result;

			if (value is CostLog costLog)
			{
				var costType = Costs.GetType(Costs.Types, costLog.CostTypeId ?? string.Empty);
				var categoryName = Costs.GetCategory(costType.Category);

				var bleachPercent = string.Empty;
				if ((costType?.Id ?? string.Empty) == Costs.TYPE_BLEACH && costLog.BleachPercent.HasValue)
					bleachPercent = " • " + costLog.BleachPercent.Value.ToString("0.##") + "%";

				var desc = string.Empty;
				if ((costType?.Id ?? string.Empty) == Costs.TYPE_MISC && !string.IsNullOrEmpty(costLog?.Description))
					desc = " • " + (costLog?.Description ?? string.Empty);

				//var amt = "?";
				//if (chemLog?.Amount.HasValue ?? false)
				//amt = chemLog.Amount.Value.ToString();
				//result.Spans.Add(new Span { Text = amt + " " + (unitType?.Abbreviation ?? ""), FontAttributes = FontAttributes.Bold });

				result.Add(
					costLog.Cost.HasValue ? costLog.Cost.Value.ToString("C") : "Expense",
					fontWeight: Models.FontWeight.Stronger);

				result.Add(" on ");

				result.Add(costType?.Name ?? string.Empty);

				if (!string.IsNullOrEmpty(bleachPercent))
					result.Add(bleachPercent);

				if ((costType?.Id ?? string.Empty) == Costs.TYPE_MISC && !string.IsNullOrEmpty(costLog.Description))
					result.Add($" • {costLog.Description}");
			}
			else
			{
				result.Add($"COST LOG {value}");
			}

			return result.ToFormattedString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
