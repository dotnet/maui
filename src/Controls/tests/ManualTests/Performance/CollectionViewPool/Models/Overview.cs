using System;
using Newtonsoft.Json;

namespace PoolMath.Data;

public static class OverviewExtensions
{
	public static double? GetLatestValue(this Overview overview, ChemicalLevelType chemicalLevelType)
		=> overview == null ? null : chemicalLevelType switch
		{
			ChemicalLevelType.FreeChlorine => overview.FreeChlorine,
			ChemicalLevelType.Ph => overview.Ph,
			ChemicalLevelType.TotalAlkalinity => overview.TotalAlkalinity,
			ChemicalLevelType.CalciumHardness => overview.CalciumHardness,
			ChemicalLevelType.Cya => overview.Cya,
			ChemicalLevelType.Borates => overview.Borates,
			ChemicalLevelType.Salt => overview.Salt,
			ChemicalLevelType.None => default,
			ChemicalLevelType.Csi => overview.CSI,
			ChemicalLevelType.WaterTemp => overview.WaterTemp,
			ChemicalLevelType.CombinedChlorine => overview.CombinedChlorine,
			_ => default,
		};

	public static DateTime? GetLatestTimestamp(this Overview overview, ChemicalLevelType chemicalLevelType)
		=> overview == null ? null : chemicalLevelType switch
		{
			ChemicalLevelType.FreeChlorine => overview.LastFC,
			ChemicalLevelType.Ph => overview.LastPH,
			ChemicalLevelType.TotalAlkalinity => overview.LastTA,
			ChemicalLevelType.CalciumHardness => overview.LastCH,
			ChemicalLevelType.Cya => overview.LastCYA,
			ChemicalLevelType.Borates => overview.LastBorates,
			ChemicalLevelType.Salt => overview.LastSalt,
			ChemicalLevelType.None => default,
			ChemicalLevelType.Csi => overview.LastCSI,
			ChemicalLevelType.WaterTemp => overview.LastWaterTemp,
			ChemicalLevelType.CombinedChlorine => overview.LastCC,
			_ => default,
		};


	//public static (double? target, double? targetMin, double? targetMax, double? idealMin, double? idealMax) GetRecommendedLevels(this Overview overview, Pool pool, ChemicalLevelType chemicalLevelType)
	//{
	//	if (overview is null || pool is null)
	//		return (null, null, null, null, null);

	//	var sv = pool.GetSuggestedValues();

	//	return chemicalLevelType switch
	//	{
	//		ChemicalLevelType.FreeChlorine
	//			=> (sv.FreeChlorineTarget, sv.FreeChlorineMin, sv.FreeChlorineMax, sv.FreeCholorineIdealMin, sv.FreeCholorineIdealMax),
	//		ChemicalLevelType.Ph
	//			=> (sv.PhTarget, sv.PhMin, sv.PhMax, sv.PhIdealMin, sv.PhIdealMax),
	//		ChemicalLevelType.TotalAlkalinity
	//			=> (sv.TotalAlkalinityTarget, sv.TotalAlkalinityMin, sv.TotalAlkalinityMax, sv.TotalAlkalinityIdealMin, sv.TotalAlkalinityIdealMax),
	//		ChemicalLevelType.CalciumHardness
	//			=> (sv.CalciumHardnessTarget, sv.CalciumHardnessMin, sv.CalciumHardnessMax, sv.CalciumHardnessIdealMin, sv.CalciumHardnessIdealMax),
	//		ChemicalLevelType.Cya
	//			=> (sv.CyaTarget, sv.CyaMin, sv.CyaMax, sv.CyaIdealMin, sv.CyaIdealMax),
	//		ChemicalLevelType.Borates
	//			=> (null, null, null, null, null),
	//		ChemicalLevelType.Salt
	//			=> (null, null, null, null, null),
	//		ChemicalLevelType.None
	//			=> (null, null, null, null, null),
	//		ChemicalLevelType.Csi
	//			=> (sv.CsiTarget, sv.CsiMin, sv.CsiMax, sv.CsiIdealMin, sv.CsiIdealMax),
	//		ChemicalLevelType.WaterTemp
	//			=> (null, null, null, null, null),
	//		ChemicalLevelType.CombinedChlorine
	//			=> (sv.CombinedChlorineTarget, sv.CombinedChlorineMin, sv.CombinedChlorineMax, null, null),
	//		_ => (null, null, null, null, null)
	//	};
	//}
}

public class Overview
{
	[JsonProperty("fc")]
	[System.Text.Json.Serialization.JsonPropertyName("fc")]
	public double? FreeChlorine { get; set; }

	[JsonProperty("cc")]
	[System.Text.Json.Serialization.JsonPropertyName("cc")]
	public double? CombinedChlorine { get; set; }

	[JsonProperty("cya")]
	[System.Text.Json.Serialization.JsonPropertyName("cya")]
	public double? Cya { get; set; }

	[JsonProperty("ch")]
	[System.Text.Json.Serialization.JsonPropertyName("ch")]
	public double? CalciumHardness { get; set; }

	[JsonProperty("ph")]
	[System.Text.Json.Serialization.JsonPropertyName("ph")]
	public double? Ph { get; set; }

	[JsonProperty("ta")]
	[System.Text.Json.Serialization.JsonPropertyName("ta")]
	public double? TotalAlkalinity { get; set; }

	[JsonProperty("salt")]
	[System.Text.Json.Serialization.JsonPropertyName("salt")]
	public double? Salt { get; set; }

	[JsonProperty("bor")]
	[System.Text.Json.Serialization.JsonPropertyName("bor")]
	public double? Borates { get; set; }

	[JsonProperty("tds")]
	[System.Text.Json.Serialization.JsonPropertyName("tds")]
	public double? TDS { get; set; }

	[JsonProperty("csi")]
	[System.Text.Json.Serialization.JsonPropertyName("csi")]
	public double? CSI { get; set; }

	[JsonProperty("flowRate")]
	[System.Text.Json.Serialization.JsonPropertyName("flowRate")]
	public double? FlowRate { get; set; }

	[JsonProperty("pressure")]
	[System.Text.Json.Serialization.JsonPropertyName("pressure")]
	public double? Pressure { get; set; }

	[JsonProperty("waterTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTemp")]
	public double? WaterTemp { get; set; }

	[JsonProperty("pumpRunTime")]
	[System.Text.Json.Serialization.JsonPropertyName("pumpRunTime")]
	public int? PumpRunTime { get; set; }

	[JsonProperty("swgCellPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("swgCellPercent")]
	public int? SWGCellPercent { get; set; }

	[JsonProperty("fcTs")]
	[System.Text.Json.Serialization.JsonPropertyName("fcTs")]
	public DateTime? LastFC { get; set; }

	[JsonProperty("fcLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("fcLogId")]
	public string LastLogIdFC { get; set; }

	[JsonProperty("ccTs")]
	[System.Text.Json.Serialization.JsonPropertyName("ccTs")]
	public DateTime? LastCC { get; set; }

	[JsonProperty("ccLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("ccLogId")]
	public string LastLogIdCC { get; set; }

	[JsonProperty("cyaTs")]
	[System.Text.Json.Serialization.JsonPropertyName("cyaTs")]
	public DateTime? LastCYA { get; set; }

	[JsonProperty("cyaLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("cyaLogId")]
	public string LastLogIdCYA { get; set; }

	[JsonProperty("chTs")]
	[System.Text.Json.Serialization.JsonPropertyName("chTs")]
	public DateTime? LastCH { get; set; }

	[JsonProperty("chLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("chLogId")]
	public string LastLogIdCH { get; set; }

	[JsonProperty("phTs")]
	[System.Text.Json.Serialization.JsonPropertyName("phTs")]
	public DateTime? LastPH { get; set; }

	[JsonProperty("phLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("phLogId")]
	public string LastLogIdPH { get; set; }

	[JsonProperty("taTs")]
	[System.Text.Json.Serialization.JsonPropertyName("taTs")]
	public DateTime? LastTA { get; set; }

	[JsonProperty("taLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("taLogId")]
	public string LastLogIdTA { get; set; }

	[JsonProperty("saltTs")]
	[System.Text.Json.Serialization.JsonPropertyName("saltTs")]
	public DateTime? LastSalt { get; set; }

	[JsonProperty("saltLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("saltLogId")]
	public string LastLogIdSalt { get; set; }

	[JsonProperty("borTs")]
	[System.Text.Json.Serialization.JsonPropertyName("borTs")]
	public DateTime? LastBorates { get; set; }

	[JsonProperty("borLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("borLogId")]
	public string LastLogIdBorates { get; set; }

	[JsonProperty("tdsTs")]
	[System.Text.Json.Serialization.JsonPropertyName("tdsTs")]
	public DateTime? LastTDS { get; set; }

	[JsonProperty("tdsLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("tdsLogId")]
	public string LastLogIdTDS { get; set; }

	[JsonProperty("csiTs")]
	[System.Text.Json.Serialization.JsonPropertyName("csiTs")]
	public DateTime? LastCSI { get; set; }

	[JsonProperty("csiLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("csiLogId")]
	public string LastLogIdCSI { get; set; }

	[JsonProperty("flowRateTs")]
	[System.Text.Json.Serialization.JsonPropertyName("flowRateTs")]
	public DateTime? LastFlowRate { get; set; }

	[JsonProperty("flowRateLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("flowRateLogId")]
	public string LastLogIdFlowRate { get; set; }

	[JsonProperty("pressureTs")]
	[System.Text.Json.Serialization.JsonPropertyName("pressureTs")]
	public DateTime? LastPressure { get; set; }

	[JsonProperty("pressureLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("pressureLogId")]
	public string LastLogIdPressure { get; set; }

	[JsonProperty("waterTempTs")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTempTs")]
	public DateTime? LastWaterTemp { get; set; }

	[JsonProperty("waterTempLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTempLogId")]
	public string LastLogIdWaterTemp { get; set; }

	[JsonProperty("pumpRuntimeTs")]
	[System.Text.Json.Serialization.JsonPropertyName("pumpRuntimeTs")]
	public DateTime? LastPumpRuntime { get; set; }

	[JsonProperty("pumpRuntimeLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("pumpRuntimeLogId")]
	public string LastLogIdPumpRuntime { get; set; }

	[JsonProperty("swgCellPercentTs")]
	[System.Text.Json.Serialization.JsonPropertyName("swgCellPercentTs")]
	public DateTime? LastSWGCellPercent { get; set; }

	[JsonProperty("swgCellPercentLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("swgCellPercentLogId")]
	public string LastLogIdSWGCellPercent { get; set; }

	[JsonProperty("backwashedTs")]
	[System.Text.Json.Serialization.JsonPropertyName("backwashedTs")]
	public DateTime? LastBackwashed { get; set; }

	[JsonProperty("backwashedLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("backwashedLogId")]
	public string LastLogIdBackwashed { get; set; }

	[JsonProperty("brushedTs")]
	[System.Text.Json.Serialization.JsonPropertyName("brushedTs")]
	public DateTime? LastBrushed { get; set; }

	[JsonProperty("brushedLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("brushedLogId")]
	public string LastLogIdBrushed { get; set; }

	[JsonProperty("cleanedFilterTs")]
	[System.Text.Json.Serialization.JsonPropertyName("cleanedFilterTs")]
	public DateTime? LastCleanedFilter { get; set; }

	[JsonProperty("cleanedFilterLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("cleanedFilterLogId")]
	public string LastLogIdCleanedFilter { get; set; }

	[JsonProperty("vacuumedTs")]
	[System.Text.Json.Serialization.JsonPropertyName("vacuumedTs")]
	public DateTime? LastVacuumed { get; set; }

	[JsonProperty("vacuumedLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("vacuumedLogId")]
	public string LastLogIdVacuumed { get; set; }


	[JsonProperty("openedTs")]
	[System.Text.Json.Serialization.JsonPropertyName("openedTs")]
	public DateTime? LastOpened { get; set; }

	[JsonProperty("openedLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("openedLogId")]
	public string LastLogIdOpened { get; set; }

	[JsonProperty("closedTs")]
	[System.Text.Json.Serialization.JsonPropertyName("closedTs")]
	public DateTime? LastClosed { get; set; }

	[JsonProperty("closedLogId")]
	[System.Text.Json.Serialization.JsonPropertyName("closedLogId")]
	public string LastLogIdClosed { get; set; }
}
