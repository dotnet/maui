using System;
using System.Text.Json;
using PoolMath;
using PoolMath.Data;

namespace PoolMathApp.Models
{
	public static class ModelExtensions
	{
		public static double? CalculateCSI(this TestLog testLog, Pool pool, Units waterTempUnits)
		{
			var ph = testLog.Ph;
			if (!ph.HasValue)
				ph = pool.Overview.Ph;
			if (!ph.HasValue)
				return null;

			var ta = testLog.TotalAlkalinity;
			if (!ta.HasValue)
				ta = pool.Overview.TotalAlkalinity;
			if (!ta.HasValue)
				return null;

			var cya = testLog.Cya;
			if (!cya.HasValue)
				cya = pool.Overview.Cya;
			if (!cya.HasValue)
				return null;

			var ch = testLog.CalciumHardness;
			if (!ch.HasValue)
				ch = pool.Overview.CalciumHardness;
			if (!ch.HasValue)
				return null;

			var temp = testLog.WaterTemp;
			if (!temp.HasValue)
				temp = pool.Overview.WaterTemp;
			if (!temp.HasValue)
				return null;

			double? salt = 1000;
			if (pool?.TrackSalt ?? false)
			{
				salt = testLog.Salt;
				if (!salt.HasValue)
					salt = pool.Overview.Salt;
				if (!salt.HasValue)
					salt = 1000;
			}

			double? bor = 0;
			if (pool?.TrackBorates ?? false)
			{
				bor = testLog.Borates;
				if (!bor.HasValue)
					bor = pool.Overview.Borates;
				if (!bor.HasValue)
					bor = 0;
			}

			var isValid = true;
			//var isValid = true;  Calculator.TryCalculateCSI(
			//	ph.Value,
			//	(int)ta.Value,
			//	(int)ch.Value,
			//	(int)cya.Value,
			//	salt.Value,
			//	bor.Value,
			//	(int)temp.Value,
			//	testLog?.WaterTempUnits ?? waterTempUnits,
			//	out var csi);

			var csi = 0.12;
			if (!isValid || double.IsNaN(csi))
				return null;

			return csi;
		}

		public static PoolMath.Units GetWaterTempUnits(this TestLog testLog, Pool pool)
		{
			if (testLog.WaterTempUnits.HasValue)
				return testLog.WaterTempUnits.Value;

			return pool?.Units ?? PoolMath.Units.US;
		}

		public static string GetFreeChlorineTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastFreeChlorine = "Target: " + 7.0.ToString("0.0") + " ↔ " + 8.0.ToString("0.0");
			if (includeLastTestInfo && pool.Overview.FreeChlorine.HasValue && pool.Overview.LastFC.HasValue)
				lastFreeChlorine += "   (" + pool.Overview.FreeChlorine.Value.ToString("0.0") + " • " + pool.Overview.LastFC.Value + ")";
			return lastFreeChlorine;
		}

		public static string GetCombinedChlorineTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastCombinedChlorine = "Target: 0.0";
			if (includeLastTestInfo && pool.Overview.CombinedChlorine.HasValue && pool.Overview.LastCC.HasValue)
				lastCombinedChlorine += "   (" + pool.Overview.CombinedChlorine.Value.ToString("0.0") + " • " + pool.Overview.LastCC.Value + ")";
			return lastCombinedChlorine;
		}

		public static string GetPhTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastPH = "Target: " + 7.0.ToString("0.0") + " ↔ " + 8.0.ToString("0.0");
			if (includeLastTestInfo && pool.Overview.Ph.HasValue && pool.Overview.LastPH.HasValue)
				lastPH += "   (" + pool.Overview.Ph.Value.ToString("0.0") + " • " + pool.Overview.LastPH + ")";
			return lastPH;
		}

		public static string GetCalciumHardnessTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastCalciumHardness = "Target: " + 0 + " ↔ " + 600;
			if (includeLastTestInfo && pool.Overview.CalciumHardness.HasValue && pool.Overview.LastCH.HasValue)
				lastCalciumHardness += "   (" + pool.Overview.CalciumHardness.Value + " • " + pool.Overview.LastCH.Value + ")";
			return lastCalciumHardness;
		}

		public static string GetTotalAlkalinityTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastTotalAlkalinity = "Target: " + 50 + " ↔ " + 100;
			if (includeLastTestInfo && pool.Overview.TotalAlkalinity.HasValue && pool.Overview.LastTA.HasValue)
				lastTotalAlkalinity += "   (" + pool.Overview.TotalAlkalinity.Value + " • " + pool.Overview.LastTA.Value + ")";
			return lastTotalAlkalinity;
		}

		public static string GetCyaTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastCYA = "Target: " + "30" + " ↔ " + "80";
			if (includeLastTestInfo && pool.Overview.Cya.HasValue && pool.Overview.LastCYA.HasValue)
				lastCYA += "   (" + pool.Overview.Cya.Value + " • " + pool.Overview.LastCYA.Value + ")";
			return lastCYA;
		}

		public static string GetSaltTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastSALT = string.Empty;
			if (pool.SaltMin.HasValue && pool.SaltMax.HasValue)
				lastSALT += "Target: " + pool.SaltMin.Value.ToString("0") + " ↔ " + pool.SaltMax.Value.ToString("0.");
			if (includeLastTestInfo && pool.Overview.Salt.HasValue && pool.Overview.LastSalt.HasValue)
				lastSALT += "   (" + pool.Overview.Salt.Value.ToString("0") + " • " + pool.Overview.LastSalt.Value + ")";
			return lastSALT.Trim();
		}

		public static string GetBoratesTargetDescription(this Pool pool, bool includeLastTestInfo = true)
		{
			var lastBor = string.Empty;
			if (pool.BoratesMin.HasValue && pool.BoratesMax.HasValue)
				lastBor += "Target: " + pool.BoratesMin.Value.ToString("0.0") + " ↔ " + pool.BoratesMax.Value.ToString("0.0");
			if (includeLastTestInfo && pool.Overview.Borates.HasValue && pool.Overview.LastBorates.HasValue)
				lastBor += "   (" + pool.Overview.Borates.Value.ToString("0.0") + " • " + pool.Overview.LastBorates.Value + ")";
			return lastBor;
		}

		public static string GetWaterTempTargetDescription(this Pool pool)
		{
			if (pool.Overview.WaterTemp.HasValue && pool.Overview.LastWaterTemp.HasValue)
				return pool.Overview.LastWaterTemp + ": " + pool.Overview.WaterTemp.Value.ToString("0") + "º";

			return string.Empty;
		}


		public static string GetPressureTargetDescription(this Pool pool)
		{
			if (pool.Overview.Pressure.HasValue && pool.Overview.LastPressure.HasValue)
				return pool.Overview.LastPressure + ": " + pool.Overview.Pressure.Value.ToString("0.0");

			return string.Empty;
		}

		public static string GetFlowRateTargetDescription(this Pool pool)
		{
			if (pool.Overview.FlowRate.HasValue && pool.Overview.LastFlowRate.HasValue)
				return pool.Overview.LastFlowRate + ": " + pool.Overview.FlowRate.Value.ToString("0.0");

			return string.Empty;
		}

		public static string GetPumpRunTimeTargetDescription(this Pool pool)
		{
			if (pool.Overview.PumpRunTime.HasValue && pool.Overview.LastPumpRuntime.HasValue)
			{
				var lastPumpRunTimeSpan = TimeSpan.FromSeconds(pool.Overview.PumpRunTime.Value);
				return pool.Overview.LastPumpRuntime + ": " + lastPumpRunTimeSpan.ToString("%h:mm");
			}

			return string.Empty;
		}


		public static string GetSWGCellPercentTargetDescription(this Pool pool)
		{
			if (pool.Overview.SWGCellPercent.HasValue && pool.Overview.LastSWGCellPercent.HasValue)
				return pool.Overview.LastSWGCellPercent + ": " + pool.Overview.SWGCellPercent.Value.ToString() + "%";

			return string.Empty;
		}

		public static string GetLastBackwashedDescription(this Pool pool)
		{
			if (pool.Overview.LastBackwashed.HasValue)
				return pool.Overview.LastBackwashed.ToString();

			return string.Empty;
		}

		public static string GetLastBrushedDescription(this Pool pool)
		{
			if (pool.Overview.LastBrushed.HasValue)
				return pool.Overview.LastBrushed.ToString();

			return string.Empty;
		}

		public static string GetLastCleanedFilterDescription(this Pool pool)
		{
			if (pool.Overview.LastCleanedFilter.HasValue)
				return pool.Overview.LastCleanedFilter.ToString();

			return string.Empty;
		}

		public static string GetLastPressureDescription(this Pool pool)
		{
			if (pool.Overview.LastPressure.HasValue)
				return pool.Overview.LastPressure.ToString();

			return string.Empty;
		}

		public static string GetLastVacuumedDescription(this Pool pool)
		{
			if (pool.Overview.LastVacuumed.HasValue)
				return pool.Overview.LastVacuumed.ToString();

			return string.Empty;
		}

		public static string GetLastClosedDescription(this Pool pool)
		{
			if (pool.Overview?.LastClosed is not null)
				return "Last Closed: " + pool.Overview.LastClosed.Value.ToString("MMM d, yyyy");

			return string.Empty;
		}

		public static string GetLastOpenedDescription(this Pool pool)
		{
			if (pool.Overview?.LastOpened is not null)
				return "Last Opened: " + pool.Overview.LastOpened.Value.ToString("MMM d, yyyy");

			return string.Empty;
		}

		public static T DeepCopy<T>(this T obj) where T : BaseDocument
		{
			T result = null;

			var json = JsonUtil.Serialize(obj);

			result = JsonUtil.Deserialize<T>(json);

			return result;
		}

	}
}
