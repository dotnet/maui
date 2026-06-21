using System;
using System.Linq;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class Pool : BaseDocument
{
	public Pool()
	{
		BuildType = PoolBuildType.Vinyl;
		ChemistryType = PoolChemistryType.Bleach;
		SanitizerType = SanitizerType.Bleach;
		PhUpType = PhUpType.Borax;
		PhDownType = PhDownType.DryAcid;
		CalciumHardnessType = CalciumHardnessType.CalciumChloride;
		CyaType = CyaType.DryStabilizer;
		PoolVolumeUnit = PoolMath.Units.US;
		WaterTempUnitDefault = PoolMath.Units.US;
		BleachPercent = 6;
		BleachJugSize = BleachJugSize.Oz128;

		TrackBackwashed = true;
		TrackCleanedFilter = true;
		TrackVacuumed = true;
		TrackWaterTemp = true;
	}

	public const string TYPE_NAME = "pool";

	[JsonProperty("type")]
	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonProperty("name")]
	[System.Text.Json.Serialization.JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonProperty("overview")]
	[System.Text.Json.Serialization.JsonPropertyName("overview")]
	public Overview Overview { get; set; } = new Overview();

	[JsonProperty("contactName")]
	[System.Text.Json.Serialization.JsonPropertyName("contactName")]
	public string ContactName { get; set; }

	[JsonProperty("contactPhone")]
	[System.Text.Json.Serialization.JsonPropertyName("contactPhone")]
	public string ContactPhone { get; set; }

	[JsonProperty("contactEmail")]
	[System.Text.Json.Serialization.JsonPropertyName("contactEmail")]
	public string ContactEmail { get; set; }

	[JsonProperty("address")]
	[System.Text.Json.Serialization.JsonPropertyName("address")]
	public string Address { get; set; }

	[JsonProperty("lat")]
	[System.Text.Json.Serialization.JsonPropertyName("lat")]
	public double? Latitude { get; set; }

	[JsonProperty("lon")]
	[System.Text.Json.Serialization.JsonPropertyName("lon")]
	public double? Longitude { get; set; }

	[JsonProperty("notes")]
	[System.Text.Json.Serialization.JsonPropertyName("notes")]
	public string Notes { get; set; }

	[JsonProperty("volume")]
	[System.Text.Json.Serialization.JsonPropertyName("volume")]
	public long Volume { get; set; }

	[JsonProperty("poolVolumeUnit")]
	[System.Text.Json.Serialization.JsonPropertyName("poolVolumeUnit")]
	public Units PoolVolumeUnit { get; set; }

	[JsonProperty("buildType")]
	[System.Text.Json.Serialization.JsonPropertyName("buildType")]
	public PoolBuildType BuildType { get; set; }

	[JsonProperty("chemType")]
	[System.Text.Json.Serialization.JsonPropertyName("chemType")]
	public PoolChemistryType ChemistryType { get; set; }

	[JsonProperty("sanitizerType")]
	[System.Text.Json.Serialization.JsonPropertyName("sanitizerType")]
	public SanitizerType SanitizerType { get; set; }

	[JsonProperty("phUpType")]
	[System.Text.Json.Serialization.JsonPropertyName("phUpType")]
	public PhUpType PhUpType { get; set; }

	[JsonProperty("phDownType")]
	[System.Text.Json.Serialization.JsonPropertyName("phDownType")]
	public PhDownType PhDownType { get; set; }

	[JsonProperty("chType")]
	[System.Text.Json.Serialization.JsonPropertyName("chType")]
	public CalciumHardnessType CalciumHardnessType { get; set; }

	[JsonProperty("cyaType")]
	[System.Text.Json.Serialization.JsonPropertyName("cyaType")]
	public CyaType CyaType { get; set; }

	[JsonProperty("borType")]
	[System.Text.Json.Serialization.JsonPropertyName("borType")]
	public BoratesType BoratesType { get; set; }

	[JsonProperty("bleachPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("bleachPercent")]
	public double BleachPercent { get; set; }

	[JsonProperty("bleachJugSize")]
	[System.Text.Json.Serialization.JsonPropertyName("bleachJugSize")]
	public BleachJugSize BleachJugSize { get; set; }

	[JsonProperty("fcTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("fcTarget")]
	public double? FreeChlorineTarget { get; set; }

	[JsonProperty("fcSLAMTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("fcSLAMTarget")]
	public double? FreeChlorineSLAMTarget { get; set; }

	[JsonProperty("cyaTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("cyaTarget")]
	public double? CyaTarget { get; set; }

	[JsonProperty("phTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("phTarget")]
	public double? PhTarget { get; set; }

	[JsonProperty("taTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("taTarget")]
	public double? TotalAlkalinityTarget { get; set; }

	[JsonProperty("chTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("chTarget")]
	public double? CalciumHardnessTarget { get; set; }

	[JsonProperty("saltMin")]
	[System.Text.Json.Serialization.JsonPropertyName("saltMin")]
	public double? SaltMin { get; set; }

	[JsonProperty("saltMax")]
	[System.Text.Json.Serialization.JsonPropertyName("saltMax")]
	public double? SaltMax { get; set; }

	[JsonProperty("saltTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("saltTarget")]
	public double? SaltTarget { get; set; }

	[JsonProperty("borMin")]
	[System.Text.Json.Serialization.JsonPropertyName("borMin")]
	public double? BoratesMin { get; set; }

	[JsonProperty("borMax")]
	[System.Text.Json.Serialization.JsonPropertyName("borMax")]
	public double? BoratesMax { get; set; }

	[JsonProperty("borTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("borTarget")]
	public double? BoratesTarget { get; set; }

	[JsonProperty("cyaEntered")]
	[System.Text.Json.Serialization.JsonPropertyName("cyaEntered")]
	public double? CyaEntered { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public bool UseCyaEntered { get; set; } = false;

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public double CsiMin { get { return -0.6d; } }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public double CsiMax { get { return 0.6d; } }

	[JsonProperty("trackSalt")]
	[System.Text.Json.Serialization.JsonPropertyName("trackSalt")]
	public bool TrackSalt { get; set; }

	[JsonProperty("trackBor")]
	[System.Text.Json.Serialization.JsonPropertyName("trackBor")]
	public bool TrackBorates { get; set; }

	[JsonProperty("trackCC")]
	[System.Text.Json.Serialization.JsonPropertyName("trackCC")]
	public bool TrackCombinedChlorine { get; set; }

	[JsonProperty("trackCSI")]
	[System.Text.Json.Serialization.JsonPropertyName("trackCSI")]
	public bool TrackCSI { get; set; }

	[JsonProperty("trackFlowRate")]
	[System.Text.Json.Serialization.JsonPropertyName("trackFlowRate")]
	public bool TrackFlowRate { get; set; }

	[JsonProperty("trackPressure")]
	[System.Text.Json.Serialization.JsonPropertyName("trackPressure")]
	public bool TrackPressure { get; set; }

	[JsonProperty("trackBackwashed")]
	[System.Text.Json.Serialization.JsonPropertyName("trackBackwashed")]
	public bool TrackBackwashed { get; set; }

	[JsonProperty("trackBrushed")]
	[System.Text.Json.Serialization.JsonPropertyName("trackBrushed")]
	public bool TrackBrushed { get; set; }

	[JsonProperty("trackVacuumed")]
	[System.Text.Json.Serialization.JsonPropertyName("trackVacuumed")]
	public bool TrackVacuumed { get; set; }

	[JsonProperty("trackCleanedFilter")]
	[System.Text.Json.Serialization.JsonPropertyName("trackCleanedFilter")]
	public bool TrackCleanedFilter { get; set; }

	[JsonProperty("trackWaterTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("trackWaterTemp")]
	public bool TrackWaterTemp { get; set; }

	[JsonProperty("trackSWGCellPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("trackSWGCellPercent")]
	public bool TrackSWGCellPercent { get; set; }

	[JsonProperty("trackPumpRunTime")]
	[System.Text.Json.Serialization.JsonPropertyName("trackPumpRunTime")]
	public bool TrackPumpRunTime { get; set; }

	[JsonProperty("remindBackwashed")]
	[System.Text.Json.Serialization.JsonPropertyName("remindBackwashed")]
	public bool RemindBackwashed { get; set; }

	[JsonProperty("remindBackwashedDays")]
	[System.Text.Json.Serialization.JsonPropertyName("remindBackwashedDays")]
	public int RemindBackwashedDays { get; set; } = 7;

	[JsonProperty("remindBrushed")]
	[System.Text.Json.Serialization.JsonPropertyName("remindBrushed")]
	public bool RemindBrushed { get; set; }

	[JsonProperty("remindBrushedDays")]
	[System.Text.Json.Serialization.JsonPropertyName("remindBrushedDays")]
	public int RemindBrushedDays { get; set; } = 7;

	[JsonProperty("remindVacuumed")]
	[System.Text.Json.Serialization.JsonPropertyName("remindVacuumed")]
	public bool RemindVacuumed { get; set; }

	[JsonProperty("remindVacuumedDays")]
	[System.Text.Json.Serialization.JsonPropertyName("remindVacuumedDays")]
	public int RemindVacuumedDays { get; set; } = 7;

	[JsonProperty("remindCleanedFilter")]
	[System.Text.Json.Serialization.JsonPropertyName("remindCleanedFilter")]
	public bool RemindCleanedFilter { get; set; }

	[JsonProperty("remindCleanedFilterDays")]
	[System.Text.Json.Serialization.JsonPropertyName("remindCleanedFilterDays")]
	public int RemindCleanedFilterDays { get; set; } = 14;

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public Units Units => PoolVolumeUnit;

	[JsonProperty("waterTempUnitDefault")]
	[System.Text.Json.Serialization.JsonPropertyName("waterTempUnitDefault")]
	public Units? WaterTempUnitDefault { get; set; } = null;

	[JsonProperty("logWeather")]
	[System.Text.Json.Serialization.JsonPropertyName("logWeather")]
	public bool LogWeather { get; set; } = true;

	[JsonProperty("weatherLocId")]
	[System.Text.Json.Serialization.JsonPropertyName("weatherLocId")]
	public string WeatherLocationId { get; set; }

	[JsonProperty("zip")]
	[System.Text.Json.Serialization.JsonPropertyName("zip")]
	public string Zip { get; set; }


	[JsonProperty("alwaysShowPoolVolume")]
	[System.Text.Json.Serialization.JsonPropertyName("alwaysShowPoolVolume")]
	public bool AlwaysShowPoolVolume { get; set; } = false;

	[JsonProperty("hideIdealRangeAlerts")]
	[System.Text.Json.Serialization.JsonPropertyName("hideIdealRangeAlerts")]
	public bool HideIdealRangeAlerts { get; set; } = false;

	[JsonProperty("hideRecommendedRangeAlerts")]
	[System.Text.Json.Serialization.JsonPropertyName("hideRecommendedRangeAlerts")]
	public bool HideRecommendedRangeAlerts { get; set; } = false;

	[JsonProperty("shareDataOptOut")]
	[System.Text.Json.Serialization.JsonPropertyName("shareDataOptOut")]
	public bool ShareDataOptOut { get; set; } = false;

	[JsonProperty("shareWithCode")]
	[System.Text.Json.Serialization.JsonPropertyName("shareWithCode")]
	public bool ShareWithCode { get; set; }

	[JsonProperty("shareCode")]
	[System.Text.Json.Serialization.JsonPropertyName("shareCode")]
	public string ShareCode { get; set; }

	[JsonProperty("shareWithTfp")]
	[System.Text.Json.Serialization.JsonPropertyName("shareWithTfp")]
	public bool ShareWithTfp { get; set; }

	[JsonProperty("swgLbsPerDay")]
	[System.Text.Json.Serialization.JsonPropertyName("swgLbsPerDay")]
	public double? SwgLbsPerDay { get; set; }

	[JsonProperty("swgModelId")]
	[System.Text.Json.Serialization.JsonPropertyName("swgModelId")]
	public string SwgModelId { get; set; }

	[JsonProperty("overrideFCTarget")]
	[System.Text.Json.Serialization.JsonPropertyName("overrideFCTarget")]
	public double? OverrideFCTarget { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public SwgModel SwgModel
	{
		get { return SwgModel.Types.FirstOrDefault(t => t.Id == SwgModelId); }
		set
		{
			SwgModelId = value?.Id;
		}
	}

	public string CalculateShareCode()
	{
		if (string.IsNullOrEmpty(Id))
			return null;

		try
		{
			var hashCode = Math.Abs(Id.GetHashCode(StringComparison.Ordinal));
			return hashCode.ToString();
		}
		catch { }

		return null;
	}

	public void GenerateShareCode()
	{
		if (!ShareWithCode)
		{
			ShareCode = null;
			return;
		}

		ShareCode = CalculateShareCode();

		if (string.IsNullOrEmpty(ShareCode))
			ShareWithCode = false;
	}

	public string GetShareUrl()
		=> string.IsNullOrEmpty(ShareCode) ? null : "https://troublefreepool.com/mypool/" + ShareCode;

	public override string ToString()
	{
		return string.Format("[PoolConfig: Id={0}]", Id);
	}

	//ChemicalTypes.SuggestedValues suggestedValues;
	//[JsonIgnore]
	//[System.Text.Json.Serialization.JsonIgnore]
	//public ChemicalTypes.SuggestedValues SuggestedValues
	//{
	//	get
	//	{
	//		if (suggestedValues == null)
	//			suggestedValues = GetSuggestedValues();
	//		return suggestedValues;
	//	}
	//	set
	//	{
	//		suggestedValues = value;
	//	}
	//}


	//public void UpdateSuggestedTargets(bool overwriteExisting = true)
	//{
	//	var sv = GetSuggestedValues();

	//	if (!FreeChlorineTarget.HasValue || FreeChlorineTarget <= 0 || overwriteExisting)
	//		FreeChlorineTarget = sv.FreeChlorineTarget;
	//	FreeChlorineSLAMTarget = sv.FreeChlorineShock;
	//	if (!PhTarget.HasValue || PhTarget <= 0) // || overwriteExisting)
	//		PhTarget = sv.PhTarget;
	//	if (!CalciumHardnessTarget.HasValue || CalciumHardnessTarget <= 0) // || overwriteExisting)
	//		CalciumHardnessTarget = sv.CalciumHardnessTarget;
	//	if (!TotalAlkalinityTarget.HasValue || TotalAlkalinityTarget <= 0) // || overwriteExisting)
	//		TotalAlkalinityTarget = sv.TotalAlkalinityTarget;
	//	if (!CyaTarget.HasValue || CyaTarget <= 0) // || overwriteExisting)
	//		CyaTarget = sv.CyaTarget;

	//	//suggestedValues = sv;
	//}

	//public ChemicalTypes.SuggestedValues GetSuggestedValues()
	//{
	//	//if (suggestedValues == null) {

	//	double lastCyaValue = 0;

	//	// Non-Subscribers will likely not have previous test logs
	//	// or if they do, they're probably stale, so we shouldn't use that value
	//	//if (!SubscriptionService.Instance.IsSubscribed) {
	//	//    lastCyaValue = CyaEntered ?? 0;
	//	//} else {


	//	if (UseCyaEntered)
	//		lastCyaValue = CyaEntered ?? 0;
	//	else
	//		lastCyaValue = Overview.Cya ?? CyaEntered ?? 0;
	//	//}

	//	return ChemicalTypes.GetSuggestedValues(BuildType, ChemistryType, lastCyaValue);
	//}

	public bool IsVacuumingOverdue()
	{
		return this.RemindVacuumed
				   && (DateTime.UtcNow - (this.Overview?.LastVacuumed ?? DateTime.MinValue)).TotalDays
				   > this.RemindVacuumedDays;
	}

	public bool IsBrushingOverdue()
	{
		return this.RemindBrushed
				   && (DateTime.UtcNow - (this.Overview?.LastBrushed ?? DateTime.MinValue)).TotalDays
				   > this.RemindBrushedDays;
	}

	public bool IsBackwashingOverdue()
	{
		return this.RemindBackwashed
				   && (DateTime.UtcNow - (this.Overview?.LastBackwashed ?? DateTime.MinValue)).TotalDays
				   > this.RemindBackwashedDays;
	}

	public bool IsCleanFilterOverdue()
	{
		return this.RemindCleanedFilter
				   && (DateTime.UtcNow - (this.Overview?.LastCleanedFilter ?? DateTime.MinValue)).TotalDays
				   > this.RemindCleanedFilterDays;
	}
}
