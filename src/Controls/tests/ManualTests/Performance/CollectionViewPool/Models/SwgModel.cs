using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace PoolMath.Data;

public class SwgModelGroup : List<SwgModel>
{
	public SwgModelGroup(string name, IEnumerable<SwgModel> swgModels)
	{
		Name = name;
		AddRange(swgModels);
	}

	public SwgModelGroup(string name, string description, IEnumerable<SwgModel> swgModels)
	{
		Name = name;
		Description = description;
		AddRange(swgModels);
	}

	public string Name { get; set; }
	public string Description { get; set; }
}

public class SwgModel : BaseDocument
{
	public SwgModel()
	{
	}

	public SwgModel(string id, string manufacturer, string model, double lbsPerDay)
	{
		Id = id;
		Manufacturer = manufacturer;
		Model = model;
		LbsPerDay = lbsPerDay;
	}

	public static SwgModel Custom(double lbsPerDay)
			=> new SwgModel("custom", "Not Listed", "Custom Value", lbsPerDay);

	public static SwgModel Empty
			=> new SwgModel("custom", "Not Listed", "Custom Value", 0d);



	[JsonProperty("manufacturer")]
	[System.Text.Json.Serialization.JsonPropertyName("manufacturer")]
	public string Manufacturer { get; set; }

	[JsonProperty("model")]
	[System.Text.Json.Serialization.JsonPropertyName("model")]
	public string Model { get; set; }

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public string Text =>
		Manufacturer + " - " + Model;

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public string Name =>
		Text;

	[JsonProperty("lbsPerDay")]
	[System.Text.Json.Serialization.JsonPropertyName("lbsPerDay")]
	public double LbsPerDay { get; set; }

	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => "swgtype";

	public static SwgModel GetModel(string modelId)
	{
		if (string.IsNullOrEmpty(modelId))
			return null;

		foreach (var m in Types)
		{
			if (m.Id == modelId)
				return m;
		}

		return null;
	}

	public static List<SwgModelGroup> ModelsByManufacturer
	{
		get
		{
			var groups = SwgModel.Types
				.GroupBy(t => t.Manufacturer)
				.Select(g => new SwgModelGroup(g.Key, g))
				.ToList();
			groups.Insert(0, new SwgModelGroup("Not Listed", new[] { SwgModel.Empty }));
			return groups;
		}
	}

	public const string CustomModelId = "custom";

	public static List<SwgModel> Types = new List<SwgModel>
	{

		new SwgModel("astral-viron-v18", "Astral", "Viron V18", 0.952),
		new SwgModel("astral-viron-v25", "Astral", "Viron V25", 1.322),
		new SwgModel("astral-viron-v35", "Astral", "Viron V35", 1.852),
		new SwgModel("astral-viron-v45", "Astral", "Viron V45", 2.381),
		new SwgModel("astral-viron-eq-18", "Astral", "Viron eQuilibrium EQ18", 0.952),
		new SwgModel("astral-viron-eq-25", "Astral", "Viron eQuilibrium EQ25", 1.322),
		new SwgModel("astral-viron-eq-35", "Astral", "Viron eQuilibrium EQ35", 1.852),
		new SwgModel("astral-viron-eq-45", "Astral", "Viron eQuilibrium EQ45", 2.381),
		new SwgModel("astral-vx-7s", "Astral", "VX 7S", 1.322),
		new SwgModel("astral-vx-7t", "Astral", "VX 7T", 1.322),
		new SwgModel("astral-vx-9s", "Astral", "VX 9S", 1.587),
		new SwgModel("astral-vx-9t", "Astral", "VX 9T", 1.587),
		new SwgModel("astral-vx-11s", "Astral", "VX 11S", 2.222),
		new SwgModel("astral-vx-11t", "Astral", "VX 11T", 2.222),

		new SwgModel("atlantis-atl-21", "Atlantis", "ATL-21", 1.4),

		new SwgModel("autochlor-smc-20", "Auto Chlor", "SMC-20", 1.05),
		new SwgModel("autochlor-smc-30", "Auto Chlor", "SMC-30", 1.58),

		new SwgModel("autopilot-digital-nanoplus", "Autopilot", "Digital Nano+ PPC2", 1.06),
		new SwgModel("autopilot-digital-ppc1", "Autopilot", "Digital PPC1 (RC-35)", 1.28),
		new SwgModel("autopilot-digital-ppc3", "Autopilot", "Digital PPC3 (RC-42)", 1.52),
		new SwgModel("autopilot-digital-ppc4", "Autopilot", "Digital PPC4 (RC-52)", 2.0),
		new SwgModel("autopilot-tc-ppc1", "Autopilot", "Total Control PPC1", 1.28),
		new SwgModel("autopilot-tc-ppc3", "Autopilot", "Total Control PPC3", 1.52),
		new SwgModel("autopilot-tc-ppc4", "Autopilot", "Total Control PPC4", 2.0),

		new SwgModel("circupool-core15", "CircuPool", "Core-15", 0.9),
		new SwgModel("circupool-core35", "CircuPool", "Core-35", 1.4),
		new SwgModel("circupool-core55", "CircuPool", "Core-55", 2.3),

		new SwgModel("circupool-edge15", "CircuPool", "Edge-15", 0.7),
		new SwgModel("circupool-edge25", "CircuPool", "Edge-25", 1.2),
		new SwgModel("circupool-edge40", "CircuPool", "Edge-40", 1.7),

		new SwgModel("circupool-u40", "CircuPool", "Universal40", 2.0),
		new SwgModel("circupool-sj15", "CircuPool", "SJ-15", 0.65),
		new SwgModel("circupool-sj20", "CircuPool", "SJ-20", 0.87),
		new SwgModel("circupool-sj40", "CircuPool", "SJ-40", 1.7),
		new SwgModel("circupool-sj55", "CircuPool", "SJ-55", 2.35),
		new SwgModel("circupool-rj16", "CircuPool", "RJ-16", 0.7),
		new SwgModel("circupool-rj20", "CircuPool", "RJ-20", 0.9),
		new SwgModel("circupool-rj30", "CircuPool", "RJ-30", 1.5),
		new SwgModel("circupool-rj45", "CircuPool", "RJ-45", 2.0),
		new SwgModel("circupool-rj60", "CircuPool", "RJ-60", 3.1),
		new SwgModel("circupool-rj16p", "CircuPool", "RJ-16 Plus", 0.7),
		new SwgModel("circupool-rj20p", "CircuPool", "RJ-20 Plus", 0.9),
		new SwgModel("circupool-rj30p", "CircuPool", "RJ-30 Plus", 1.5),
		new SwgModel("circupool-rj45p", "CircuPool", "RJ-45 Plus", 2.0),
		new SwgModel("circupool-rj60p", "CircuPool", "RJ-60 Plus", 3.1),

		new SwgModel("compupool-compuchlor-a25", "Compu Pool", "Compu Chlor A25", 1.3),
		new SwgModel("compupool-compuchlor-a35", "Compu Pool", "Compu Chlor A35", 1.95),
		new SwgModel("compupool-cpx-cpsc16", "Compu Pool", "CPX / CPSC16", 0.56),
		new SwgModel("compupool-cpx-cpsc24", "Compu Pool", "CPX / CPSC24", 0.67),
		new SwgModel("compupool-cpx-cpsc36", "Compu Pool", "CPX / CPSC36", 0.92),
		new SwgModel("compupool-cpx-cpsc48", "Compu Pool", "CPX / CPSC48", 1.28),

		new SwgModel("hayward-ss3c", "Hayward", "Salt & Swim 3C", 0.98),
		new SwgModel("hayward-aquarite-t3", "Hayward", "Aqua Rite (T-3)", 0.53),
		new SwgModel("hayward-aquarite-t9", "Hayward", "Aqua Rite (T-9)", 0.98),
		new SwgModel("hayward-aquarite-t15", "Hayward", "Aqua Rite (T-15)", 1.47),
		new SwgModel("hayward-aquarite-t15pro", "Hayward", "Aqua Rite Pro (T-15)", 1.47),

		new SwgModel("hayward-turbo-cell-t5", "Hayward", "Turbo Cell (T-CELL-5)", 0.7),

		new SwgModel("intermatic-ipure-pe25k", "Intermatic", "I-Pure PE25k", 1.1),
		new SwgModel("intermatic-ipure-pe40k", "Intermatic", "I-Pure PE40k", 1.7),

		new SwgModel("intex-krystal-clear", "Intex", "Krystal Clear", 0.63),

		new SwgModel("jandy-aquapure-700", "Jandy", "Aquapure 700", 0.63),
		new SwgModel("jandy-aquapure-1400", "Jandy", "Aquapure 1400", 1.25),
		new SwgModel("jandy-truclear-ei", "Jandy", "Truclear / Ei", 0.92),

		new SwgModel("monarch-esc16-esc6000", "Monarch", "ESC16 / ESC6000", 0.84),
		new SwgModel("monarch-esc24-esc7000", "Monarch", "ESC24 / ESC7000", 1.24),
		new SwgModel("monarch-esc36-esc8000", "Monarch", "ESC36 / ESC8000", 1.9),

		new SwgModel("pentair-ic-15", "Pentair", "Intellichlor IC-15", 0.6),
		new SwgModel("pentair-ic-20", "Pentair", "Intellichlor IC-20", 0.7),
		new SwgModel("pentair-ic-30", "Pentair", "Intellichlor IC-30", 1.0),
		new SwgModel("pentair-ic-40", "Pentair", "Intellichlor IC-40", 1.4),
		new SwgModel("pentair-ic-60", "Pentair", "Intellichlor IC-60", 2.0),

		new SwgModel("pentair-ichlor-15", "Pentair", "iChlor 15", 0.6),
		new SwgModel("pentair-ichlor-30", "Pentair", "iChlor 30", 1.0),

		new SwgModel("pureline-crystal-pure-20", "Pureline", "Crystal Pure 20,000", 1.06),
		new SwgModel("pureline-crystal-pure-40", "Pureline", "Crystal Pure 40,000", 2.1),
		new SwgModel("pureline-crystal-pure-60", "Pureline", "Crystal Pure 60,000", 3.18),

		new SwgModel("sgs_breeze320", "Saline Generating Systems", "Breeze 320", 0.7),
		new SwgModel("sgs_breeze540", "Saline Generating Systems", "Breeze 540", 1.35),
		new SwgModel("sgs_breeze760", "Saline Generating Systems", "Breeze 760", 1.88),

		new SwgModel("solaxx-reliant-purechlor-r4", "Solaxx (Saltron)", "Reliant / Purechlor R4", 0.75),
		new SwgModel("solaxx-reliant-purechlor-r5", "Solaxx (Saltron)", "Reliant / Purechlor R5", 1.15),
		new SwgModel("solaxx-reliant-purechlor-r7", "Solaxx (Saltron)", "Reliant / Purechlor R7", 1.65),
		new SwgModel("solaxx-resilience-aquacomfort-a3", "Solaxx (Saltron)", "Resilience / Aquacomfort A3", 0.75),
		new SwgModel("solaxx-resilience-aquacomfort-a5", "Solaxx (Saltron)", "Resilience / Aquacomfort A5", 1.38),
		new SwgModel("solaxx-resilience-aquacomfort-a7", "Solaxx (Saltron)", "Resilience / Aquacomfort A7", 2.07),
		new SwgModel("solaxx-nexa-pure-18k", "Solaxx (Saltron)", "Nexa Pure 18k", 0.75),
		new SwgModel("solaxx-nexa-pure-40k", "Solaxx (Saltron)", "Rexa Pure 40k", 1.38),


		new SwgModel("watermaid-ez300", "Watermaid", "EZ300", 1.58),

		new SwgModel("zodiac-lm3-15", "Zodiac", "LM3-15", 0.79),
		new SwgModel("zodiac-lm3-24", "Zodiac", "LM3-24", 1.27),
		new SwgModel("zodiac-lm3-40", "Zodiac", "LM3-40", 2.11),
	};
}
