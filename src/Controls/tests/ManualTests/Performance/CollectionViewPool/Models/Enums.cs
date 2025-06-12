using System;

namespace PoolMath;

public enum Units
{
	US, //0
	Metric, // 1
	Imperial // 2
}

public enum LengthUnitOfMeasurement
{
	Feet,
	Inches,
	Meters,
	Centimeters
}

public enum VolumeUnitOfMeasurement
{
	Milliliters,
	Liters,
	FluidOunces,
	Gallons,
	ImperialFluidOunces,
	ImperialGallons
}

public enum UnitOfMeasurement
{
	Milliliters,
	Liters,
	Grams,
	Kilograms,
	FluidOunces,
	Gallons,
	Pounds,
	Ounces,
	Ppm
}

public enum PoolShape
{
	Rectangle,
	Circular,
	Oblong,
	Triangular
}

public enum PoolBuildType
{
	Vinyl,
	Plaster,
	Fiberglass
}

public enum PoolChemistryType
{
	Bleach,
	SWG
}

public enum SanitizerType
{
	Bleach,
	SWG,
	Trichlor,
	Dichlor,
	CalHypo48,
	CalHypo53,
	CalHypo65,
	CalHypo73,
	LithiumHypo,
	ChlorineGas,
	CalHypo70,
}

public enum PhUpType
{
	SodaAsh,
	Borax,
	//Stp,
	//CausticSoda,
}

public enum PhDownType
{
	Baume15,
	Baume28,
	Baume31,
	Baume34,
	MuriaticAcid14,
	MuriaticAcid29,
	DryAcid
}

public enum CyaType
{
	DryStabilizer,
	LiquidStabilizer,
}

public enum TotalAlkalinityType
{
	BakingSoda,
}

public enum CalciumHardnessType
{
	CalciumChloride,
	CalciumChlorideDihydrate,
}

public enum BoratesType
{
	Borax,
	BoricAcid,
	TetraboratePentahydrate
}

public enum BleachJugSize
{
	Ml2000 = 2000,
	Ml2850 = 2850,
	Ml3600 = 3600,
	Ml5000 = 5000,
	Ml887 = 887,
	Ml1893 = 1893,
	Ml3578 = 3578,
	Oz81 = 81,
	Oz96 = 96,
	Oz128 = 128,
	Oz174 = 174,
	Oz182 = 182,
	Oz30 = 30,
	Oz64 = 64,
	Oz121 = 121,
}

public enum ChemicalAdditionType
{
	Bleach,
	Trichlor,
	Dichlor,
	CalHypo48,
	CalHypo53,
	CalHypo65,
	CalHypo73,
	LithiumHypo,
	ChlorineGas,
	Borax,
	BoricAcid,
	SodaAsh,
	Baume10,
	Baume28,
	Baume31,
	Baume34,
	MuriaticAcid14,
	MuriaticAcid29,
	MuriaticAcid31, // This one is obsolete
	BakingSoda,
	DryAcid,
	TetraboratePentahydrate,
	CalciumChloride,
	CalciumChlorideDihydrate,
	DryStabilizer,
	LiquidStabilizer,
	Salt,
	SWG,
	Polyquat,
	CalHypo70,
}

public enum ChemicalLevelType
{
	FreeChlorine,
	Ph,
	TotalAlkalinity,
	CalciumHardness,
	Cya,
	Borates,
	Salt,
	None,
	Csi,
	CombinedChlorine,
	WaterTemp
}

public enum TemperatureUnit
{
	Farenheit,
	Celsius
}


public enum PoolVolumeUnit
{
	Gallons,
	Liters,
	ImpGallons
}

public enum BleachVolumeUnit
{
	Ounces,
	Gallons,
	Milliliters,
	Liters
}

public enum ChemicalState
{
	Either,
	Liquid,
	Solid
}
