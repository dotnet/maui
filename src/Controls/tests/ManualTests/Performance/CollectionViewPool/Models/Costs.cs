using System.Collections.Generic;
using System.Linq;

namespace PoolMath.Data;

public static class Costs
{
	public const string TYPE_BLEACH = "fc-bleach";

	public const string TYPE_MISC = "misc";

	public const string TYPE_WATER = "water";
	public const string TYPE_NATURAL_GAS = "natgas";
	public const string TYPE_PROPANE = "propane";
	public const string TYPE_POWER = "power";

	public const string UNIT_GALLONS = "Gallons";
	public const string UNIT_LITERS = "Liters";
	public const string UNIT_MILLILITERS = "Milliliters";
	public const string UNIT_CCF = "CCF";
	public const string UNIT_CUBIC_FEET = "Cubic Feet";

	public const string UNIT_GRAMS = "Grams";
	public const string UNIT_KG = "Kilograms";
	public const string UNIT_OZ = "Ounces";
	public const string UNIT_LBS = "Pounds";

	public static CostType GetType(IEnumerable<CostType> availableTypes, string typeId)
	{
		return availableTypes.FirstOrDefault(t => t.Id == typeId);
	}

	public static string GetCategory(CostTypeCategory category)
	{
		switch (category)
		{
			case CostTypeCategory.Chemicals_PhUp:
				return "Chemicals - pH Up";
			case CostTypeCategory.Chemicals_PhDown:
				return "Chemicals - pH Down";
			case CostTypeCategory.Chemicals_Sanitizer:
				return "Chemicals - Sanitizer";
			case CostTypeCategory.Chemicals_Stabilizer:
				return "Chemicals - Stabilizer";
			case CostTypeCategory.Chemicals_Ta:
				return "Chemicals - Total Alkalinity";
			case CostTypeCategory.Chemicals_Ch:
				return "Chemicals - Calcium Hardness";
			case CostTypeCategory.Chemicals_Bor:
				return "Chemicals - Borates";
			case CostTypeCategory.Chemicals_Misc:
				return "Chemicals - Miscellaneous";
			case CostTypeCategory.Equipment:
				return "Equipment";
			case CostTypeCategory.Utilities:
				return "Utilities";
			case CostTypeCategory.Other:
				return "Miscellaneous";
			default:
				return "Miscellaneous";
		}
	}

	public static readonly List<string> WeightMeasurementUnits =
		new List<string> { UNIT_GRAMS, UNIT_KG, UNIT_OZ, UNIT_LBS };

	public static readonly List<string> VolumeMeasurementUnits =
		new List<string> { UNIT_MILLILITERS, UNIT_LITERS, UNIT_OZ, UNIT_GALLONS };

	public static List<CostType> Types = new List<CostType>  {
		new CostType(TYPE_BLEACH, "Liquid Chlorine / Bleach", CostTypeCategory.Chemicals_Sanitizer, true) {
			MeasurementUnits = VolumeMeasurementUnits,
			MeasurementType = MeasurementType.Volume
		},
		new CostType("fc-dichlor", "Dichlor powder", CostTypeCategory.Chemicals_Sanitizer, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("fc-trichlor", "Trichlor tablets/pucks", CostTypeCategory.Chemicals_Sanitizer, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("fc-calchypochlorite", "Calcium Hypochlorite (Cal-Hypo)", CostTypeCategory.Chemicals_Sanitizer, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("ph-up-phup", "pH-Up (soda ash, sodium carbonate, washing soda)", CostTypeCategory.Chemicals_PhUp, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("ph-up-borax", "Borax", CostTypeCategory.Chemicals_PhUp, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("ph-down-dryacid", "Dry Acid (pH-Down, sodium bisulfate)", CostTypeCategory.Chemicals_PhDown, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("ph-down-muriaticacid", "Muriatic acid", CostTypeCategory.Chemicals_PhDown, true) {
			MeasurementUnits = VolumeMeasurementUnits,
			MeasurementType = MeasurementType.Volume
		},

		new CostType("ta-taup", "Alkalinity increaser (baking soda)", CostTypeCategory.Chemicals_Ta, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("ch-chup", "Calcium increaser", CostTypeCategory.Chemicals_Ch, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("bor-boricacid", "Boric acid", CostTypeCategory.Chemicals_Bor, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("bor-borax", "Borax", CostTypeCategory.Chemicals_Bor, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("bor-proteam", "ProTeam Supreme", CostTypeCategory.Chemicals_Bor, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("chemmisc-salt", "Salt", CostTypeCategory.Chemicals_Misc, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},

		new CostType("stabilizer-dry", "Dry Stabilizer", CostTypeCategory.Chemicals_Stabilizer, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("stabilizer-liquid", "Liquid Stabilizer", CostTypeCategory.Chemicals_Stabilizer, true) {
			MeasurementUnits = VolumeMeasurementUnits,
			MeasurementType = MeasurementType.Volume
		},



		new CostType("chemmisc-sand", "Sand", CostTypeCategory.Chemicals_Misc, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("chemmisc-de", "Diatomaceous Earth", CostTypeCategory.Chemicals_Misc, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("chemmisc-ascorbicacid", "Ascorbic Acid", CostTypeCategory.Chemicals_Misc, true) {
			MeasurementUnits = WeightMeasurementUnits,
			MeasurementType = MeasurementType.Weight
		},
		new CostType("chemmisc-polyquat", "Polyquat (algaecide)", CostTypeCategory.Chemicals_Misc, true) {
			MeasurementUnits = VolumeMeasurementUnits,
			MeasurementType = MeasurementType.Volume
		},

		new CostType(TYPE_POWER, "Power / Electricity", CostTypeCategory.Utilities, trackAmount: true) {
			MeasurementUnits = new List<string> { "kWh" },
			MeasurementType = MeasurementType.Custom
		},
		new CostType(TYPE_WATER, "Water", CostTypeCategory.Utilities, trackAmount: true) {
			MeasurementUnits = new List<string> { UNIT_CCF, UNIT_GALLONS, UNIT_LITERS },
			MeasurementType = MeasurementType.Custom
		},
		new CostType(TYPE_NATURAL_GAS, "Natural Gas", CostTypeCategory.Utilities, trackAmount: true) {
			MeasurementUnits = new List<string> { "Therms", UNIT_CUBIC_FEET },
			MeasurementType = MeasurementType.Custom
		},
		new CostType(TYPE_PROPANE, "Propane", CostTypeCategory.Utilities, trackAmount: true) {
			MeasurementUnits = new List<string> { UNIT_GALLONS },
			MeasurementType = MeasurementType.Custom
		},

		new CostType("toys", "Pool Toys", CostTypeCategory.Equipment),
		new CostType("acc", "Accessories", CostTypeCategory.Equipment),

		new CostType("misc-prosvcs", "Professional Services", CostTypeCategory.Other),
		new CostType("t-kit", "Test Kit Reagents", CostTypeCategory.Other),
		new CostType("misc", "Other", CostTypeCategory.Other),
	};
}
