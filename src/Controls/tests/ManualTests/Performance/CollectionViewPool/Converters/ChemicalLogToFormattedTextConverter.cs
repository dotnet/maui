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
	public class ChemicalLogToFormattedTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var ft = new FormattedText();

			var chemLog = value as ChemicalLog;
			if (chemLog != null)
			{
				var chemicalType = chemLog.Chemical;
				if (chemicalType == ChemicalAdditionType.Baume31)
					chemicalType = ChemicalAdditionType.MuriaticAcid31;

				if (chemicalType.HasValue)
				{
					if (chemicalType.HasValue && chemicalType.Value == ChemicalAdditionType.SWG)
					{
						var ppm = chemLog.Amount ?? 0;
						var time = chemLog.RunTime ?? 0;
						var swgPerc = chemLog.Percent ?? 100;

						ft = new FormattedText(
							defaultFontWeight: Models.FontWeight.Lighter,
							spans: new[] {
							new TextSpan(ppm.ToString("0.#") + " " + "ppm", fontWeight: Models.FontWeight.Stronger,  colorKey: "PrimaryTextColor"),
							new TextSpan(" FC", colorKey: "FreeChlorineAccentColor", fontWeight: Models.FontWeight.Stronger),
							new TextSpan(" • SWG "),
							new TextSpan(time.ToString("0.#"), fontWeight: Models.FontWeight.Strong,  colorKey: "PrimaryTextColor"),
							new TextSpan(" hrs @ "),
							new TextSpan(swgPerc.ToString("0"), fontWeight: Models.FontWeight.Strong,  colorKey: "PrimaryTextColor"),
							new TextSpan("%")
							});
					}
					else
					{
						//var chemType = ChemicalTypes.GetChemicalAdditionType(chemicalType ?? ChemicalAdditionType.Bleach);

						//if (chemType is not null)
						//{
						//var unitType = ChemicalTypes.UnitsOfMeasurement.FirstOrDefault(um => um.Value == chemLog.Unit && um.State == chemType.State) ?? ChemicalTypes.UnitsOfMeasurement.First(); ;
						var chemColor = "PrimaryTextColor";

						var bleachPercent = string.Empty;
						if ((chemicalType == ChemicalAdditionType.Bleach || chemicalType == ChemicalAdditionType.Polyquat)
							&& chemLog.Percent.HasValue)
							bleachPercent = " • " + chemLog.Percent.Value.ToString("0.##") + "%";

						var amt = "?";
						if (chemLog?.Amount.HasValue ?? false)
							amt = chemLog.Amount.Value.ToString();

						ft = new FormattedText(
							defaultFontWeight: Models.FontWeight.Lighter,
							spans: new[] {
							new TextSpan(amt + " " + ("oz"), fontWeight: Models.FontWeight.Stronger, colorKey: "PrimaryTextColor"),
							new TextSpan(" of "),
							new TextSpan("Some Chemical", colorKey: chemColor)
							});

						if (!string.IsNullOrEmpty(bleachPercent))
							ft.Add(bleachPercent, colorKey: chemColor, fontWeight: Models.FontWeight.Strong);
						//}
					}
				}
			}

			return ft.ToFormattedString();
		}



		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
